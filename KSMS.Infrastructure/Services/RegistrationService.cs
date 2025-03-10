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
using System.Security.Claims;
using System.Linq.Expressions;
using Hangfire;
using KSMS.Application.Extensions;
using KSMS.Domain.Common;
using KSMS.Domain.Pagination;
using KSMS.Domain.Dtos.Responses.KoiMedium;

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

    public async Task AssignMultipleFishesToTankAndRound(Guid roundId, List<Guid> registrationIds)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
            var tankRepository = _unitOfWork.GetRepository<Tank>();
            var registrationRepository = _unitOfWork.GetRepository<Registration>();

            // 1️ Kiểm tra danh sách rỗng
            if (registrationIds == null || !registrationIds.Any())
            {
                throw new ArgumentException("Registration list cannot be empty.");
            }

            // 2️ Lấy danh sách đơn đăng ký
            var registrations = await registrationRepository.GetListAsync(
                predicate: r => registrationIds.Contains(r.Id));

            // 3️ Kiểm tra cùng hạng mục
            var categoryId = registrations.First().CompetitionCategoryId;
            if (registrations.Any(r => r.CompetitionCategoryId != categoryId))
            {
                throw new Exception("All registrations must belong to the same category.");
            }

            // 4️ Kiểm tra vòng thi hợp lệ
            var roundExists = (await _unitOfWork.GetRepository<Round>().GetListAsync(predicate: r => r.Id == roundId 
            && r.CompetitionCategoriesId == categoryId && r.Status == "Active")).Any();


            if (!roundExists)
            {
                throw new Exception($"Round {roundId} is not valid for category {categoryId}.");
            }

            // 5️ Lấy danh sách hồ khả dụng
            var availableTanks = await tankRepository.GetListAsync(
                predicate: t => t.KoiShowId == registrations.First().KoiShowId && t.Status == "Available");

            if (!availableTanks.Any())
            {
                throw new Exception("No available tanks found. Please add more tanks before assigning fishes.");
            }

            // 6️ Tìm hồ trống có cùng hạng mục
            Tank? selectedTank = null;
            foreach (var tank in availableTanks)
            {
                if (!await _tankService.IsTankFull(tank.Id))
                {
                    selectedTank = tank;
                    break;
                }
            }

            // 7️ Nếu không có hồ nào chứa được, báo lỗi
            if (selectedTank == null)
            {
                throw new Exception("All available tanks are full. Please free up space or add more tanks.");
            }

            // 8️ Gán toàn bộ cá vào cùng 1 hồ và 1 round
            var newRegisRounds = registrations.Select(registration => new RegistrationRound
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                RoundId = roundId,
                TankId = selectedTank.Id,
                CheckInTime = VietNamTimeUtil.GetVietnamTime(),
                Status = "Assigned",
                CreatedAt = VietNamTimeUtil.GetVietnamTime()
            }).ToList();

            await regisRoundRepository.InsertRangeAsync(newRegisRounds);
            await _unitOfWork.CommitAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception("Failed to assign fishes.", ex);
        }
    }


     
    public async Task<CheckQRRegistrationResoponse> GetRegistrationByIdAndRoundAsync(Guid registrationId, Guid roundId)
    {
        var registrationRepository = _unitOfWork.GetRepository<Registration>();

        var registration = await registrationRepository.SingleOrDefaultAsync(
            predicate: r => r.Id == registrationId &&
                            r.RegistrationRounds.Any(rr => rr.RoundId == roundId), // Thêm điều kiện kiểm tra RoundId
            include: query => query
                .Include(r => r.KoiMedia)
                .ThenInclude(km => km.KoiProfile)
                .Include(r => r.RegistrationRounds)
                .ThenInclude(rr => rr.Round) // Bao gồm thông tin về vòng thi
        );

        if (registration == null)
        {
            throw new NotFoundException("Registration not found in the specified round.");
        }

        return registration.Adapt<CheckQRRegistrationResoponse>();
    }



    public async Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest)
    {
        var accountId = GetIdFromJwt();
        var koiShow = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiShowId);
        var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiProfileId,
                include: query => query.Include(r => r.Variety)
                    .ThenInclude(r => r.CategoryVarieties).ThenInclude(r => r.CompetitionCategory));
        if (koiShow is null)
        {
            throw new NotFoundException("Show is not existed");
        }

        if (koiProfile is null)
        {
            throw new NotFoundException("Koi is not existed");
        }
        
        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x => x.KoiShowId == koiShow.Id && x.Status == RegistrationStatus.Confirmed.ToString().ToLower());
        if (registrations.Count > koiShow.MaxParticipants)
        {
            throw new NotFoundException("");
        }
        var eligibleCategories =koiProfile.Variety.CategoryVarieties
            .Select(cv => cv.CompetitionCategory)
            .Where(cc => 
                koiProfile.Size >= cc.SizeMin && 
                koiProfile.Size <= cc.SizeMax &&
                cc.KoiShowId == createRegistrationRequest.KoiShowId)
            .ToList();

        if (!eligibleCategories.Any())
            throw new BadRequestException("No suitable category was found for this Koi fish");
        var bestCategory = eligibleCategories.MinBy(c => c.SizeMax - c.SizeMin);
        if (bestCategory != null)
        {
            var registrationCount =
                await _unitOfWork.GetRepository<Registration>()
                    .GetListAsync(predicate: x => x.CompetitionCategoryId == bestCategory.Id && x.Status == RegistrationStatus.Confirmed.ToString().ToLower());
            if (registrationCount.Count > bestCategory.MaxEntries)
            {
                throw new NotFoundException("The number of participants in the category exceeds the limit");
            }

            var registration = createRegistrationRequest.Adapt<Registration>();
            registration.KoiAge = koiProfile.Age;
            registration.KoiSize = koiProfile.Size;
            registration.RegistrationFee = koiShow.RegistrationFee;
            registration.AccountId = accountId;
            registration.CompetitionCategoryId = bestCategory.Id;
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
            // var staffList = await _unitOfWork.GetRepository<ShowStaff>()
            //     .GetListAsync(predicate: s => s.KoiShowId == koiShow.Id,
            //         include: query => query.Include(s => s.Account));
            //
            // // Gửi thông báo cho tất cả staff
            // foreach (var staff in staffList)
            // {
            //     await _notificationService.SendNotification(
            //         staff.Account.Id,
            //         "New Registration",
            //         $"New registration from {registration.Account.FullName} for koi {koiProfile.Name}",
            //         NotificationType.NewRegistration
            //     );
            // }

            return new
            {
                Id = registration.Id
            };
        }

        throw new NotFoundException("No suitable category was found for this Koi fish");
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
        if (registration is null)
        {
            throw new NotFoundException("Registration is not existed");
        }

        if (registration.Status != RegistrationStatus.Pending.ToString().ToLower())
        {
            throw new NotFoundException("This Registration is not paid");
        }
        var accountId = GetIdFromJwt();
        var userRole = GetRoleFromJwt();

        if (userRole != "ADMIN")
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
            //RegistrationStatus.NotEnoughQuota => RegistrationStatus.NotEnoughQuota.ToString().ToLower(),
            //RegistrationStatus.Cancelled => RegistrationStatus.Cancelled.ToString().ToLower(),
            _ => registration.Status
        };
        if (registration.Status == RegistrationStatus.Rejected.ToString().ToLower())
        {
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
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

            _backgroundJobClient.Enqueue(() => _emailService.SendRegistrationConfirmationEmail(registrationId));
        }
        
    }

    public async Task<CheckOutRegistrationResponse> CheckOut(Guid registrationId)
    {
        var accountId = GetIdFromJwt();
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(predicate: r => r.Id == registrationId,
                include: query => query.Include(r => r.KoiProfile)
                    .Include(r => r.KoiShow)
                    .Include(r => r.RegistrationPayment));
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
            $"Registration #{registrationId.ToString().Substring(0,8)} - {registration.KoiProfile.Name}", 
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
                include: q => q
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

            default:
                //predicate = r => r.Status != RegistrationStatus..ToString().ToLower();
                predicate = r => r.AccountId == GetIdFromJwt();
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