using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Models;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using System.Linq.Expressions;
using Hangfire;
using KSMS.Application.Extensions;
using KSMS.Domain.Common;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Pagination;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Infrastructure.Services;

public class RegistrationService : BaseService<RegistrationService>, IRegistrationService
{
    private readonly PayOS _payOs;
    private readonly IMediaService _mediaService;
    private readonly IFirebaseService _firebaseService;
    private readonly INotificationService _notificationService;
    private readonly ITankService _tankService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    public RegistrationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RegistrationService> logger,
        IHttpContextAccessor httpContextAccessor, PayOS payOs, IMediaService mediaService, IFirebaseService firebaseService, INotificationService notificationService, ITankService tankService, IBackgroundJobClient backgroundJobClient, IEmailService emailService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _payOs = payOs;
        _mediaService = mediaService;
        _firebaseService = firebaseService;
        _notificationService = notificationService;
        _tankService = tankService;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
    }

    public async Task<List<Guid>> GetRegistrationIdsByKoiShowAsync(Guid koiShowId)
    {
        var registrationRepository = _unitOfWork.GetRepository<Registration>();
        var registrationIds = await registrationRepository.GetListAsync(
            predicate: r => r.KoiShowId == koiShowId,
            selector: r => r.Id
        );

        return (List<Guid>)registrationIds;
    }


   
    public async Task<Paginate<GetPageRegistrationHistoryResponse>> GetPageRegistrationHistory(RegistrationStatus? registrationStatus, ShowStatus? showStatus, int page, int size)
    {
        Expression<Func<Registration, bool>> filterQuery = registration => registration.AccountId == GetIdFromJwt();
        if (registrationStatus.HasValue)
        {
            var status = registrationStatus.Value.ToString().ToLower();
            filterQuery = filterQuery.AndAlso(r => r.Status == status);
        }
        if (showStatus.HasValue)
        {
            var status = showStatus.Value.ToString().ToLower();
            filterQuery = filterQuery.AndAlso(r => r.KoiShow.Status == status);
        }
        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetPagingListAsync(predicate: filterQuery,
                orderBy: query => query.OrderByDescending(r => r.CreatedAt),
                include: query => query
                    .Include(r => r.KoiShow)
                    .Include(r => r.KoiProfile)
                    .ThenInclude(k => k.Variety)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.KoiMedia),
                page: page,
                size: size
            );
        return registrations.Adapt<Paginate<GetPageRegistrationHistoryResponse>>();
    }






    public async Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest)
    {
        var accountId = GetIdFromJwt();
        var koiShow = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiShowId);
        var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiProfileId);
        var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
            predicate: x => x.Id == createRegistrationRequest.CompetitionCategoryId);
        if (koiShow is null)
        {
            throw new NotFoundException("Show is not existed");
        }

        if (koiProfile is null)
        {
            throw new NotFoundException("Koi is not existed");
        }

        if (category is null)
        {
            throw new NotFoundException("Category is not existed");
        }

        var existingRegistration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.KoiShowId == koiShow.Id &&
                                r.KoiProfileId == koiProfile.Id &&
                                r.Status != RegistrationStatus.Rejected.ToString().ToLower() &&
                                r.Status != RegistrationStatus.Refunded.ToString().ToLower(),
                include: query => query.Include(r => r.CompetitionCategory));
        if (existingRegistration != null)
        {
            throw new BadRequestException("This koi has already been registered in category " + existingRegistration.CompetitionCategory.Name + " of this show");
        }
        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x => x.KoiShowId == koiShow.Id && x.Status == RegistrationStatus.Confirmed.ToString().ToLower());
        if (registrations.Count > koiShow.MaxParticipants)
        {
            throw new NotFoundException("The number of participants in this show exceeds the limit");
        }
        var registrationCount = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x =>
                x.CompetitionCategoryId == category.Id &&
                x.Status == RegistrationStatus.Confirmed.ToString().ToLower());

        if (registrationCount.Count >= category.MaxEntries)
            throw new BadRequestException("The selected category has reached maximum entries");

        var registration = createRegistrationRequest.Adapt<Registration>();
        registration.KoiAge = koiProfile.Age;
        registration.KoiSize = koiProfile.Size;
        registration.RegistrationFee = category.RegistrationFee;
        registration.AccountId = accountId;
        registration.CompetitionCategoryId = category.Id;
        registration.Status = RegistrationStatus.WaitToPaid.ToString().ToLower();
        await _unitOfWork.GetRepository<Registration>().InsertAsync(registration);
        await _unitOfWork.CommitAsync();
        if (createRegistrationRequest.RegistrationImages is not [])
        {
            await _mediaService.UploadRegistrationImage(createRegistrationRequest.RegistrationImages,
                registration.Id);
        }

        if (createRegistrationRequest.RegistrationVideos is not [])
        {
            await _mediaService.UploadRegistrationVideo(createRegistrationRequest.RegistrationVideos,
                registration.Id);
        }
        return new
        {
            Id = registration.Id
        };
    }

    // New method to find suitable category
    public async Task<GetPageCompetitionCategoryResponse> FindSuitableCategoryAsync(Guid koiShowId, Guid varietyId, decimal size)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
        .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Show is not found");
        }
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(
                predicate: v => v.Id == varietyId,
                include: query => query.Include(v => v.CategoryVarieties)
                    .ThenInclude(cv => cv.CompetitionCategory));

        if (variety == null)
            throw new NotFoundException("Variety not found");
        var categoriesForVariety = variety.CategoryVarieties
            .Select(cv => cv.CompetitionCategory)
            .Where(cc => cc.KoiShowId == koiShowId)
            .ToList();
            
        if (!categoriesForVariety.Any())
            throw new BadRequestException("No suitable category was found for the variety ofD this Koi fish");
        var eligibleCategories = categoriesForVariety
            .Where(cc => size >= cc.SizeMin && size <= cc.SizeMax)
            .ToList();

        if (!eligibleCategories.Any())
        {
            if (categoriesForVariety.Any())
                throw new BadRequestException("No suitable category was found for the size of this Koi fish");
            throw new BadRequestException("No suitable category was found for the variety and size of this Koi fish");
        }
        var bestCategory = eligibleCategories.MinBy(c => c.SizeMax - c.SizeMin);
        if (bestCategory == null)
            throw new BadRequestException("No suitable category was found for this Koi fish");
        
        return bestCategory.Adapt<GetPageCompetitionCategoryResponse>();
    }



    public async Task UpdateRegistrationPaymentStatusForPayOs(Guid registrationPaymentId, RegistrationPaymentStatus status)
    {
        var registrationPayment = await _unitOfWork.GetRepository<RegistrationPayment>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationPaymentId,
                include: query => query
                    .Include(r => r.Registration));
        
        registrationPayment.Status = status switch
        {
            RegistrationPaymentStatus.Cancelled => RegistrationPaymentStatus.Cancelled.ToString().ToLower(),
            RegistrationPaymentStatus.Paid => RegistrationPaymentStatus.Paid.ToString().ToLower(),
            _ => registrationPayment.Status
        };
        if (registrationPayment.Status == RegistrationStatus.Cancelled.ToString().ToLower())
        {
            registrationPayment.Registration.Status = RegistrationStatus.Cancelled.ToString().ToLower();
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registrationPayment);
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registrationPayment.Registration);
            await _unitOfWork.CommitAsync();
        }
        if (registrationPayment.Status == RegistrationPaymentStatus.Paid.ToString().ToLower())
        {
            registrationPayment.Registration.Status = RegistrationStatus.Pending.ToString().ToLower();
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registrationPayment);
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registrationPayment.Registration);
            await _unitOfWork.CommitAsync();
            var koiShow = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(
                predicate: x => x.Id == registrationPayment.Registration.KoiShowId);
            var staffList = await _unitOfWork.GetRepository<ShowStaff>()
                .GetListAsync(predicate: s => s.KoiShowId == registrationPayment.Registration.KoiShowId,
                    include: query => query.Include(s => s.Account));
            await _notificationService.SendNotificationToMany(staffList.Select(s => s.AccountId).ToList(),
                "Thông báo đăng ký mới",
                $"Có một đơn đăng ký mới tham gia triển lãm Koi: {koiShow.Name}. Vui lòng kiểm tra chi tiết.",
                NotificationType.Registration
            );
            await _notificationService.SendNotification(registrationPayment.Registration.AccountId,
                "Đăng ký thành công",
                $"Bạn đã đăng ký thành công tham gia triển lãm {koiShow.Name}. Đơn đăng ký của bạn sẽ được nhân viên duyệt và chi tiết sẽ gửi qua mail.",
                NotificationType.Registration);
            _backgroundJobClient.Enqueue(() => _emailService.SendPaymentConfirmationEmail(registrationPaymentId));
        }
        
    }
    public async Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status)
    {

        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include:
                query => query.Include(r => r.RegistrationPayment));
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate:
            s => s.Id == registration.KoiShowId);
        if (registration is null)
        {
            throw new NotFoundException("Registration is not existed");
        }
        //
        // if (registration.Status != RegistrationStatus.Pending.ToString().ToLower())
        // {
        //     throw new NotFoundException("This Registration is not paid");
        // }
        var accountId = GetIdFromJwt();
        var userRole = GetRoleFromJwt();

        if (userRole != "Admin")
        {
            var showStaff = await _unitOfWork.GetRepository<ShowStaff>()
                .SingleOrDefaultAsync(predicate: s => s.AccountId == accountId && s.KoiShowId == registration.KoiShowId);

            if (showStaff is null)
            {
                throw new ForbiddenMethodException("You are not authorized to update this registration.");
            }
        }
        registration.Status = status switch
        {
            RegistrationStatus.Confirmed => RegistrationStatus.Confirmed.ToString().ToLower(),
            RegistrationStatus.Rejected => RegistrationStatus.Rejected.ToString().ToLower(),
            RegistrationStatus.CheckIn => RegistrationStatus.CheckIn.ToString().ToLower(),
            RegistrationStatus.Refunded => RegistrationStatus.Refunded.ToString().ToLower(),
            _ => registration.Status
        };
        if (registration.Status == RegistrationStatus.Rejected.ToString().ToLower())
        {
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
            await _notificationService.SendNotification(registration.AccountId,
                "Đơn đăng kí của bạn đã bị từ chối",
                " Đơn đăng kí tham gia triễn lãm " + registration.KoiShow.Name + "của bạn đã bị từ chối. Vui lòng xem email để biết thêm chi tiết",
                NotificationType.Registration);
            _backgroundJobClient.Enqueue(() => _emailService.SendRegistrationRejectionEmail(registrationId));
        }
        if (registration.Status == RegistrationStatus.Confirmed.ToString().ToLower())
        {
            registration.ApprovedAt = VietNamTimeUtil.GetVietnamTime();

            // Generate QR code
            var qrCodeData = QrcodeUtil.GenerateQrCode(registration.RegistrationPayment.Id);
            registration.RegistrationPayment.QrcodeData = await _firebaseService.UploadImageAsync(
                FileUtils.ConvertBase64ToFile(qrCodeData),
                "qrCode/"
            );
            registration.ApprovedAt = VietNamTimeUtil.GetVietnamTime();

            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registration.RegistrationPayment);
            await _unitOfWork.CommitAsync();
            await _notificationService.SendNotification(registration.AccountId,
                "Đăng kí của bạn đã được chấp nhận",
                " Đơn đăng kí tham gia triễn lãm " + registration.KoiShow.Name + "của bạn đã được chấp nhận",
                NotificationType.Registration);
            _backgroundJobClient.Enqueue(() => _emailService.SendRegistrationConfirmationEmail(registrationId));
        }
        if (registration.Status == RegistrationStatus.CheckIn.ToString().ToLower())
        {
            registration.CheckInTime = VietNamTimeUtil.GetVietnamTime();
            registration.IsCheckedIn = true;
            registration.CheckInLocation = show.Location;
            registration.CheckedInBy = accountId;
            registration.QrcodeData = await _firebaseService.UploadImageAsync(
                FileUtils.ConvertBase64ToFile(QrcodeUtil.GenerateQrCode(registration.Id)), "qrCode/");
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
            await _notificationService.SendNotification(registration.AccountId,
                "Đăng kí của bạn đã được check in",
                " Đơn đăng kí tham gia triễn lãm " + registration.KoiShow.Name + "của bạn đã được check in thành công",
                NotificationType.Registration);
        }

        if (registration.Status == RegistrationStatus.Refunded.ToString().ToLower())
        {
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
            await _notificationService.SendNotification(registration.AccountId,
                "Phí đăng ký của bạn đã được hoàn tiền",
                "Đơn đăng ký tham gia triển lãm " + registration.KoiShow.Name + " đã được hoàn phí đăng ký. Vui lòng kiểm tra tài khoản của bạn trong vòng 3-5 ngày làm việc. Chi tiết đã được gửi qua email.",
                NotificationType.Registration);
            _backgroundJobClient.Enqueue(() => _emailService.SendRefundEmail(registrationId));
        }

    }

    public async Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId)
    {
        var accountId = GetIdFromJwt();
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationId,
                include: query => query.Include(r => r.KoiProfile)
                    .Include(r => r.KoiShow)
                    .Include(r => r.RegistrationPayment)
                    .Include(r => r.CompetitionCategory));
        if (registration is null)
        {
            throw new NotFoundException("Registration is not found");
        }

        if (registration.AccountId != accountId)
        {
            throw new ForbiddenMethodException("This registration is not yours!!!!");
        }
        if (registration.Status == RegistrationStatus.Pending.ToString().ToLower())
        {
            throw new BadRequestException("This registration is already paid and pending to staff!!!");
        }
        var timestamp = DateTimeOffset.Now.ToString("yyMMddHHmmss");
        var random = new Random().Next(1000, 9999).ToString(); //
        var registrationCode = long.Parse($"{timestamp}{random}");
        RegistrationPayment registrationPayment;
        if (registration.RegistrationPayment != null)
        {
            registration.RegistrationPayment.PaymentDate = VietNamTimeUtil.GetVietnamTime();
            registration.RegistrationPayment.TransactionCode = registrationCode.ToString();
            registration.RegistrationPayment.Status = RegistrationPaymentStatus.Pending.ToString().ToLower();
            registration.RegistrationPayment.PaidAmount = registration.RegistrationFee;
            _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registration.RegistrationPayment);
            registrationPayment = registration.RegistrationPayment;
        }
        else
        {
            registrationPayment = new RegistrationPayment
            {
                RegistrationId = registration.Id,
                TransactionCode = registrationCode.ToString(),
                Status = RegistrationPaymentStatus.Pending.ToString().ToLower(),
                PaymentMethod = PaymentMethod.PayOs.ToString(),
                PaidAmount = registration.RegistrationFee,
                PaymentDate = VietNamTimeUtil.GetVietnamTime()
            };
            await _unitOfWork.GetRepository<RegistrationPayment>().InsertAsync(registrationPayment);

        }

        await _unitOfWork.CommitAsync();
        var items = new List<ItemData>();
        var item = new ItemData(
            $"Registration #{registrationId.ToString().Substring(0, 8)} - {registration.KoiProfile.Name}",
            1,
            (int)registration.RegistrationFee
        );
        items.Add(item);

        var baseUrl = $"{AppConfig.AppSetting.BaseUrl}/api/v1/registration" + "/call-back";
        var url = $"{baseUrl}?registrationPaymentId={registrationPayment.Id}";

        var paymentData = new PaymentData(
            registrationCode,
            (int)registration.RegistrationFee,
            $"Registration",
            items,
            url,
            url
        );

        var createPayment = await _payOs.createPaymentLink(paymentData);
        
       
        return new CheckOutRegistrationResponse()
        {
            Message = "Create payment Successfully",
            Url = createPayment.checkoutUrl
        };
    }

    public async Task<Paginate<GetRegistrationResponse>> GetAllRegistrationForCurrentMember(RegistrationFilter filter, int page, int size)
    {

        var role = GetRoleFromJwt();

        var predicate = await GetRolePredicate(role);

        predicate = ApplyFilter(predicate, filter);

        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetPagingListAsync(
                predicate: predicate,
                orderBy: q => q.OrderByDescending(r => r.CreatedAt),
                include: q => q.AsSplitQuery()
                    .Include(r => r.KoiShow)
                    .Include(r => r.KoiProfile)
                        .ThenInclude(k => k.Variety)
                    .Include(r => r.CompetitionCategory)
                    .Include(r => r.KoiMedia),
                page: page,
                size: size
            );

        return registrations.Adapt<Paginate<GetRegistrationResponse>>();
    }

    private async Task<Expression<Func<Registration, bool>>> GetRolePredicate(string role)
    {
        Expression<Func<Registration, bool>> predicate = null;

        switch (role?.ToUpper())
        {
            case "ADMIN":
                break;
            case "MANAGER":
            case "STAFF":
                var staffShows = await _unitOfWork.GetRepository<ShowStaff>()
                    .GetListAsync(predicate: s => s.AccountId == GetIdFromJwt());
                var showIds = staffShows.Select(s => s.KoiShowId).ToList();
                predicate = r => showIds.Contains(r.KoiShowId);
                break;
        }

        return predicate;
    }

    private Expression<Func<Registration, bool>> ApplyFilter(Expression<Func<Registration, bool>> basePredicate, RegistrationFilter filter)
    {
        if (filter == null) return basePredicate;

        Expression<Func<Registration, bool>> filterQuery = basePredicate ?? (r => true);

        if (filter.ShowIds.Any())
        {
            filterQuery = filterQuery.AndAlso(r => filter.ShowIds.Contains(r.KoiShowId));
        }
        if (filter.KoiProfileIds.Any())
        {
            filterQuery = filterQuery.AndAlso(r => filter.KoiProfileIds.Contains(r.KoiProfileId));
        }
        if (filter.CategoryIds.Any())
        {
            filterQuery = filterQuery.AndAlso(r =>
                filter.CategoryIds.Contains(r.CompetitionCategoryId));
        }
        if (filter.Status.HasValue)
        {
            var statusString = filter.Status.Value.ToString().ToLower();
            filterQuery = filterQuery.AndAlso(r => r.Status == statusString);
        }

        return filterQuery;
    }


}