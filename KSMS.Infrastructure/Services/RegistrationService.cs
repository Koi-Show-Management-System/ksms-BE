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
using KSMS.Domain.Dtos.Responses.KoiMedium;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Dtos.Responses.RegistrationPayment;
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
                    .Include(r => r.KoiMedia)
                    .Include(r => r.RegistrationPayment),
                page: page,
                size: size
            );
        return registrations.Adapt<Paginate<GetPageRegistrationHistoryResponse>>();
    }

    public async Task<GetShowMemberDetailResponse> GetMemberRegisterShowDetail(Guid showId)
    {
        var currentAccount = GetIdFromJwt();
        var show = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(
                predicate: s => s.Id == showId,
                include: query => query.AsSplitQuery()
                    .Include(s => s.CompetitionCategories)
                    .ThenInclude(c => c.Awards));
        if (show is null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }

        var memberRegistrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                predicate: r => r.AccountId == currentAccount && r.KoiShowId == showId,
                include: query => query.AsSplitQuery()
                    .Include(r => r.KoiProfile)
                        .ThenInclude(kp => kp.Variety)
                    .Include(r => r.KoiMedia)
                    .Include(r => r.CompetitionCategory)
                        .ThenInclude(c => c.Awards)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.RoundResults)
                    .Include(r => r.RegistrationPayment)
                    .Include(r => r.CheckOutLog)
                        .ThenInclude(r => r.CheckedOutByNavigation)
                    .Include(r => r.Votes));
        if (!memberRegistrations.Any())
        {
            throw new NotFoundException("Ban chưa đăng ký tham gia cuộc thi này");
        }

        var allShowRegistrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                predicate: r => r.KoiShowId == showId && r.Votes.Any(),
                include: query => query.Include(r => r.Votes));
        var maxVotes = allShowRegistrations.Any() ? allShowRegistrations.Max(r => r.Votes.Count) : 0;
        var response = new GetShowMemberDetailResponse()
        {
            ShowId = show.Id,
            ShowName = show.Name,
            ShowImageUrl = show.ImgUrl,
            Location = show.Location,
            Duration = $"{show.StartDate:dd/MM/yyyy} - {show.EndDate:dd/MM/yyyy}",
            Description = show.Description,
            Status = show.Status,
            CancellationReason = show.CancellationReason,
            TotalRegisteredKoi = memberRegistrations.Count,
        };
        foreach (var registration in memberRegistrations)
        {
            string[] roundTypeOrder = {RoundEnum.Final.ToString(), RoundEnum.Evaluation.ToString(), RoundEnum.Preliminary.ToString()};
            string currentRound = null;
            foreach (var roundType in roundTypeOrder)
            {
                var latestRound = registration.RegistrationRounds
                    .Where(rr => rr.Round.RoundType == roundType)
                    .OrderByDescending(rr => rr.Round.RoundOrder)
                    .FirstOrDefault();
                if (latestRound != null)
                {
                    currentRound = latestRound.Round.RoundType;
                    break;
                }
            }
            var finalRound = registration.RegistrationRounds
                .Where(rr => rr.Round.RoundType == RoundEnum.Final.ToString())
                .OrderByDescending(rr => rr.Round.RoundOrder)
                .FirstOrDefault();
            var hasCompleteFinalRound = false;
            if (finalRound != null)
            {
                hasCompleteFinalRound = finalRound.RoundResults.Any(rr => rr.IsPublic == true);
            }

            var regDetail = new RegistrationDetailItems
            {
                RegistrationId = registration.Id,
                RegistrationNumber = registration.RegistrationNumber,
                Status = registration.Status,
                RefundType = registration.RefundType,
                RejectedReason = registration.RejectedReason,
                KoiProfileId = registration.KoiProfileId,
                KoiName = registration.KoiProfile.Name,
                Variety = registration.KoiProfile.Variety.Name,
                Size = registration.KoiSize,
                Age = registration.KoiAge,
                Gender = registration.KoiProfile.Gender,
                BloodLine = registration.KoiProfile.Bloodline,
                CategoryId = registration.CompetitionCategoryId,
                CategoryName = registration.CompetitionCategory.Name,
                RegistrationFee = registration.RegistrationFee,
                Rank = registration.Rank,
                CurrentRound = currentRound,
                Payment = registration.RegistrationPayment.Adapt<RegistrationPaymentGetRegistrationResponse>(),
                Media = registration.KoiMedia.Adapt<List<GetKoiMediaResponse>>(),
                CheckOutLog = registration.CheckOutLog.Adapt<CheckOutKoiResponse>()
            };
            if (registration.Status == "eliminated" && registration.RegistrationRounds.Any())
            {
                var failedRound = registration.RegistrationRounds
                    .Where(rr => rr.RoundResults.Any(result => result.Status == "Fail"))
                    .OrderBy(rr => GetRoundTypeOrder(rr.Round.RoundType))
                    .ThenBy(rr => rr.Round.RoundOrder)
                    .FirstOrDefault();

                if (failedRound?.Round != null)
                {
                    string roundType = GetRoundTypeDisplayName(failedRound.Round.RoundType);
                    string roundName = !string.IsNullOrEmpty(failedRound.Round.Name) 
                        ? failedRound.Round.Name 
                        : (failedRound.Round.RoundOrder.HasValue ? $"Vòng {failedRound.Round.RoundOrder}" : "");
                    
                    regDetail.EliminatedAtRound = $"{roundType}-{roundName}";
                }
            }
            if (hasCompleteFinalRound && registration.Rank.HasValue)
            {
                var awardType = registration.Rank.Value switch
                {
                    1 => "first",
                    2 => "second",
                    3 => "third",
                    4 => "honorable",
                    _ => null
                };
                var award = registration.CompetitionCategory.Awards
                    .FirstOrDefault(a => a.AwardType == awardType);
                if (award != null)
                {
                    regDetail.Awards.Add(new AwardResponse
                    {
                        CategoryName = registration.CompetitionCategory.Name,
                        AwardType = award.AwardType,
                        PrizeValue = award.PrizeValue,
                        AwardName = award.Name
                    }); 
                }
            }

            if (!show.EnableVoting &&
                registration.Votes.Any() &&
                registration.Votes.Count == maxVotes &&
                maxVotes > 0)
            {
                regDetail.Awards.Add(new AwardResponse
                {
                    CategoryName = null,
                    AwardType = "peoples_choice",
                    PrizeValue = null,
                    AwardName = "Giải bình chọn khán giả"
                });
            }
            response.Registrations.Add(regDetail);
        }

        return response;
    }
    
    private int GetRoundTypeOrder(string roundType)
    {
        return roundType?.ToLower() switch
        {
            var type when type == RoundEnum.Preliminary.ToString().ToLower() => 1,
            var type when type == RoundEnum.Evaluation.ToString().ToLower() => 2, // Semifinal = Evaluation
            var type when type == RoundEnum.Final.ToString().ToLower() => 3,
            _ => 99 // Các loại vòng thi khác (nếu có)
        };
    }

    private string GetRoundTypeDisplayName(string roundType)
    {
        return roundType switch
        {
            var type when type.Equals(RoundEnum.Preliminary.ToString(), StringComparison.OrdinalIgnoreCase) => "Vòng sơ khảo",
            var type when type.Equals(RoundEnum.Evaluation.ToString(), StringComparison.OrdinalIgnoreCase) => "Vòng đánh giá",
            var type when type.Equals(RoundEnum.Final.ToString(), StringComparison.OrdinalIgnoreCase) => "Vòng chung kết",
            _ => $"{roundType}"
        };
    }


    public async Task<object> CreateRegistration(CreateRegistrationRequest createRegistrationRequest)
    {
        var accountId = GetIdFromJwt();
        var koiShow = await _unitOfWork.GetRepository<KoiShow>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiShowId,
                include: query => query.Include(k => k.ShowStatuses));
                
        if (koiShow is null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        
        // Kiểm tra người dùng đã đăng ký show nào diễn ra cùng thời gian chưa
        var userRegistrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(
                predicate: r => r.AccountId == accountId && 
                              r.Status != RegistrationStatus.Rejected.ToString().ToLower() &&
                              r.Status != RegistrationStatus.Refunded.ToString().ToLower() &&
                              r.Status != RegistrationStatus.Cancelled.ToString().ToLower() &&
                              r.Status != RegistrationStatus.WaitToPaid.ToString().ToLower(),
                include: query => query.Include(r => r.KoiShow)
            );
            
        if (userRegistrations.Any())
        {
            // Kiểm tra nếu có show nào đã đăng ký trùng thời gian với show đang đăng ký
            foreach (var existRegistration in userRegistrations)
            {
                // Bỏ qua nếu là đăng ký cho cùng một show
                if (existRegistration.KoiShowId == koiShow.Id)
                    continue;
                    
                var existingShow = existRegistration.KoiShow;
                
                // Kiểm tra xem các show có trùng thời gian không
                bool hasTimeOverlap = (koiShow.StartDate <= existingShow.EndDate && 
                                     koiShow.EndDate >= existingShow.StartDate);
                                     
                if (hasTimeOverlap)
                {
                    throw new BadRequestException(
                        $"Bạn đã đăng ký tham gia triển lãm '{existingShow.Name}' diễn ra từ " +
                        $"{existingShow.StartDate:dd/MM/yyyy} đến {existingShow.EndDate:dd/MM/yyyy}. " +
                        $"Không thể đăng ký triển lãm '{koiShow.Name}' vì diễn ra trong cùng khoảng thời gian."
                    );
                }
            }
        }
        
        var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
            .SingleOrDefaultAsync(predicate: k => k.Id == createRegistrationRequest.KoiProfileId);
        var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
            predicate: x => x.Id == createRegistrationRequest.CompetitionCategoryId);
        
        if (koiProfile is null)
        {
            throw new NotFoundException("Không tìm thấy cá Koi");
        }

        if (category is null)
        {
            throw new NotFoundException("Không tìm thấy hạng mục");
        }

        if (koiShow.Status == ShowStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Triển lãm đã bị hủy. Không thể đăng ký tham gia");
        }
        
        // Kiểm tra xem hiện tại có đang trong giai đoạn đăng ký không
        var currentTime = VietNamTimeUtil.GetVietnamTime();
        var registrationOpenStatus = koiShow.ShowStatuses
            .FirstOrDefault(s => s.StatusName == ShowProgress.RegistrationOpen.ToString());
            
        if (registrationOpenStatus == null)
        {
            throw new BadRequestException("Triển lãm chưa mở đăng ký");
        }
        
        if (currentTime < registrationOpenStatus.StartDate || 
            currentTime > registrationOpenStatus.EndDate)
        {
            throw new BadRequestException("Hiện tại không trong thời gian đăng ký tham gia triển lãm");
        }
        // Kiểm tra xem có đơn đăng ký đang chờ thanh toán không
        var waitingRegistration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.KoiShowId == koiShow.Id &&
                                r.KoiProfileId == koiProfile.Id &&
                                r.Status == RegistrationStatus.WaitToPaid.ToString().ToLower(),
                include: query => query.Include(r => r.CompetitionCategory));

        if (waitingRegistration != null)
        {
            throw new BadRequestException($"Cá Koi này đã có đơn đăng ký đang chờ thanh toán trong hạng mục {waitingRegistration.CompetitionCategory.Name}. Bạn có thể tiếp tục thanh toán hoặc chờ khoảng 10 phút sau khi quá trình thanh toán cho đơn đó hết hạn.");
        }

        // Kiểm tra xem có đơn đăng ký đã được xử lý không
        var existingRegistration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.KoiShowId == koiShow.Id &&
                                r.KoiProfileId == koiProfile.Id &&
                                r.Status != RegistrationStatus.Rejected.ToString().ToLower() &&
                                r.Status != RegistrationStatus.Refunded.ToString().ToLower() &&
                                r.Status != RegistrationStatus.Cancelled.ToString().ToLower(),
                include: query => query.Include(r => r.CompetitionCategory));
        if (existingRegistration != null)
        {
            throw new BadRequestException($"Cá Koi này đã được đăng ký trong hạng mục {existingRegistration.CompetitionCategory.Name} của cuộc thi này");
        }
        var registrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x => x.KoiShowId == koiShow.Id && x.Status == RegistrationStatus.Confirmed.ToString().ToLower());
        if (registrations.Count > koiShow.MaxParticipants)
        {
            throw new NotFoundException("Số lượng người tham gia cuộc thi đã vượt quá giới hạn");
        }
        var registrationCount = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: x =>
                x.CompetitionCategoryId == category.Id &&
                x.Status == RegistrationStatus.Confirmed.ToString().ToLower());

        if (registrationCount.Count >= category.MaxEntries)
            throw new BadRequestException("Hạng mục đã đạt số lượng đăng ký tối đa");

        var registration = createRegistrationRequest.Adapt<Registration>();
        registration.KoiAge = koiProfile.Age;
        registration.KoiSize = koiProfile.Size;
        registration.RegistrationFee = category.RegistrationFee;
        registration.AccountId = accountId;
        registration.CompetitionCategoryId = category.Id;
        registration.Status = RegistrationStatus.WaitToPaid.ToString().ToLower();
        registration.CreatedAt = VietNamTimeUtil.GetVietnamTime();
        await _unitOfWork.GetRepository<Registration>().InsertAsync(registration);
        await _unitOfWork.CommitAsync();
        
        // Đặt lịch kiểm tra hết hạn thanh toán (2 phút sau khi tạo đơn)
       
        
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
        _backgroundJobClient.Schedule(
            () => HandleExpiredRegistration(registration.Id),
            TimeSpan.FromMinutes(3)
        );
        return new
        {
            Id = registration.Id
        };
    }

    // Thêm phương thức xử lý đơn đăng ký hết hạn
    public async Task HandleExpiredRegistration(Guid registrationId)
    {
        _logger.LogInformation($"Đang kiểm tra hết hạn đơn đăng ký: {registrationId}");
        
        var registration = await _unitOfWork.GetRepository<Registration>().SingleOrDefaultAsync(
            predicate: r => r.Id == registrationId,
            include: query => query
                .Include(r => r.RegistrationPayment)
                .Include(r => r.KoiProfile)
                .Include(r => r.CompetitionCategory)
                .Include(r => r.KoiShow)
        );
        
        if (registration == null)
        {
            _logger.LogWarning($"Không tìm thấy đơn đăng ký: {registrationId}");
            return;
        }
        
        // Chỉ xử lý các đơn đăng ký có trạng thái WaitToPaid
        if (registration.Status == RegistrationStatus.WaitToPaid.ToString().ToLower())
        {
            _logger.LogInformation($"Đơn đăng ký {registrationId} đã quá hạn thanh toán, đang xóa...");
            
            // Lưu lại các thông tin cần thiết trước khi xóa
            var accountId = registration.AccountId;
            var koiProfileName = registration.KoiProfile?.Name ?? "Không xác định";
            var categoryName = registration.CompetitionCategory?.Name ?? "Không xác định";
            var showName = registration.KoiShow?.Name ?? "Không xác định";
            
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                try
                {
                    // Xóa các media liên quan đến đơn đăng ký (nếu có)
                    var registrationMedia = await _unitOfWork.GetRepository<KoiMedium>()
                        .GetListAsync(predicate: m => m.RegistrationId == registrationId);
                        
                    if (registrationMedia.Any())
                    {
                        _unitOfWork.GetRepository<KoiMedium>().DeleteRangeAsync(registrationMedia);
                    }
                    if (registration.RegistrationPayment != null)
                    {
                        _unitOfWork.GetRepository<RegistrationPayment>().DeleteAsync(registration.RegistrationPayment);
                    }
                    // Xóa đơn đăng ký
                    _unitOfWork.GetRepository<Registration>().DeleteAsync(registration);
                    
                    await _unitOfWork.CommitAsync();
                    await transaction.CommitAsync();
                    
                    _logger.LogInformation($"Đã xóa đơn đăng ký {registrationId} do quá hạn thanh toán");
                    
                    // Gửi thông báo cho người dùng
                    await _notificationService.SendNotification(
                        accountId,
                        "Đơn đăng ký cá Koi đã hết hạn",
                        $"Đơn đăng ký cá Koi {koiProfileName} ở hạng mục {categoryName} tham gia triển lãm {showName} đã bị hủy do quá thời gian thanh toán. Bạn có thể tạo đơn đăng ký mới nếu muốn.",
                        NotificationType.Registration
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, $"Lỗi khi xử lý đơn đăng ký hết hạn: {registrationId}");
                }
            }
        }
        else
        {
            _logger.LogInformation($"Đơn đăng ký {registrationId} có trạng thái {registration.Status}, không cần xử lý hết hạn");
        }
    }

    // New method to find suitable category
    public async Task<List<GetPageCompetitionCategoryResponse>> FindSuitableCategoryAsync(Guid koiShowId, Guid varietyId, decimal size)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>()
        .SingleOrDefaultAsync(predicate: s => s.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Không tìm thấy cuộc thi");
        }
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(
                predicate: v => v.Id == varietyId,
                include: query => query.Include(v => v.CategoryVarieties)
                    .ThenInclude(cv => cv.CompetitionCategory));

        if (variety == null)
            throw new NotFoundException("Không tìm thấy giống cá");
        var categoriesForVariety = variety.CategoryVarieties
            .Select(cv => cv.CompetitionCategory)
            .Where(cc => cc.KoiShowId == koiShowId)
            .ToList();
            
        if (!categoriesForVariety.Any())
            throw new BadRequestException("Không tìm thấy hạng mục phù hợp cho giống cá này");
        
        // Lọc các hạng mục phù hợp về kích thước và không có trạng thái là cancelled
        var eligibleCategories = categoriesForVariety
            .Where(cc => size >= cc.SizeMin && size <= cc.SizeMax &&
                       (cc.Status == null || 
                        cc.Status.ToLower() != CategoryStatus.Cancelled.ToString().ToLower()))
            .ToList();

        if (!eligibleCategories.Any())
        {
            if (categoriesForVariety.Any())
                throw new BadRequestException("Không tìm thấy hạng mục phù hợp với kích thước của cá");
            throw new BadRequestException("Không tìm thấy hạng mục phù hợp với giống và kích thước của cá");
        }
        var result = new List<GetPageCompetitionCategoryResponse>();
        foreach (var category in eligibleCategories)
        {
            // Lấy tất cả varieties thuộc category này
            var categoryWithVarieties = await _unitOfWork.GetRepository<CompetitionCategory>()
                .SingleOrDefaultAsync(
                    predicate: c => c.Id == category.Id,
                    include: query => query.Include(c => c.CategoryVarieties)
                        .ThenInclude(cv => cv.Variety));
                    
            var response = category.Adapt<GetPageCompetitionCategoryResponse>();
        
            // Lấy tên của tất cả các varieties cho category này
            response.Varieties = categoryWithVarieties.CategoryVarieties
                .Select(cv => cv.Variety.Name)
                .ToList();
            
            result.Add(response);
        }
    
        return result;
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
            var registration = registrationPayment.Registration;
            var koiMedia = await _unitOfWork.GetRepository<KoiMedium>()
                .GetListAsync(predicate: x => x.RegistrationId == registration.Id);
            if (koiMedia.Any())
            {
                await _mediaService.DeleteFiles(koiMedia);
            }
            _unitOfWork.GetRepository<RegistrationPayment>().DeleteAsync(registrationPayment);
            _unitOfWork.GetRepository<Registration>().DeleteAsync(registration);
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
    public async Task UpdateStatusForRegistration(Guid registrationId, RegistrationStatus status, string? rejectedReason, RefundType? refundType)
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

        if (status == RegistrationStatus.Confirmed)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == registration.CompetitionCategoryId);
            if (category == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục thi đấu");
            }
            var confirmedCount = await _unitOfWork.GetRepository<Registration>()
                .CountAsync(predicate: x => x.CompetitionCategoryId == category.Id 
                                            && x.Status == RegistrationStatus.Confirmed.ToString().ToLower()
                                            && x.Id != registrationId);
            if (confirmedCount >= category.MaxEntries)
            {
                throw new BadRequestException($"Hạng mục '{category.Name}' đã đạt số lượng đăng ký tối đa ({category.MaxEntries}). Không thể xác nhận thêm.");
            }
        }
        registration.Status = status switch
        {
            RegistrationStatus.Confirmed => RegistrationStatus.Confirmed.ToString().ToLower(),
            RegistrationStatus.Rejected => RegistrationStatus.Rejected.ToString().ToLower(),
            RegistrationStatus.CheckIn => RegistrationStatus.CheckIn.ToString().ToLower(),
            RegistrationStatus.Refunded => RegistrationStatus.Refunded.ToString().ToLower(),
            RegistrationStatus.Cancelled => RegistrationStatus.Cancelled.ToString().ToLower(),
            _ => registration.Status
        };
        if (registration.Status == RegistrationStatus.Cancelled.ToString().ToLower())
        {
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
        }
        if (registration.Status == RegistrationStatus.Rejected.ToString().ToLower())
        {
            if (string.IsNullOrEmpty(rejectedReason))
            {
                throw new BadRequestException("Vui lòng nhập lý do từ chối");
            }
            registration.RejectedReason = rejectedReason;
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
            
            // Lấy thông tin KoiProfile và Category riêng khi cần gửi thông báo
            var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
                .SingleOrDefaultAsync(predicate: k => k.Id == registration.KoiProfileId);
                
            var category = await _unitOfWork.GetRepository<CompetitionCategory>()
                .SingleOrDefaultAsync(predicate: c => c.Id == registration.CompetitionCategoryId);
                
            await _notificationService.SendNotification(registration.AccountId,
                "Đơn đăng ký cá Koi bị từ chối",
                $"Đơn đăng ký cá Koi {koiProfile.Name} ở hạng mục {category.Name} tham gia triển lãm {show.Name} của bạn đã bị từ chối. Vui lòng kiểm tra email để biết thêm chi tiết.",
                NotificationType.Registration);
            _backgroundJobClient.Enqueue(() => _emailService.SendRegistrationRejectionEmail(registrationId, rejectedReason));
        }
        if (registration.Status == RegistrationStatus.Confirmed.ToString().ToLower())
        {
            registration.ApprovedAt = VietNamTimeUtil.GetVietnamTime();
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == registration.CompetitionCategoryId);
            var koiShow = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(
                predicate: x => x.Id == registration.KoiShowId);
            var confirmedCount = await _unitOfWork.GetRepository<Registration>()
                .CountAsync(predicate: x => x.CompetitionCategoryId == category.Id &&
                                            x.KoiShowId == registration.KoiShowId &&
                                            x.Status == RegistrationStatus.Confirmed.ToString().ToLower());
            registration.RegistrationNumber = $"{GetShowCode(koiShow)}-{GetCategoryPrefix(category)}{confirmedCount:D3}";
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
            
            // Lấy thông tin KoiProfile riêng khi cần gửi thông báo
            var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
                .SingleOrDefaultAsync(predicate: k => k.Id == registration.KoiProfileId);
                
            await _notificationService.SendNotification(registration.AccountId,
                "Đơn đăng ký cá Koi được chấp nhận",
                $"Đơn đăng ký cá Koi {koiProfile.Name} ở hạng mục {category.Name} tham gia triển lãm {show.Name} của bạn đã được chấp nhận. Mã đăng ký của bạn là {registration.RegistrationNumber}.",
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
            
            // Lấy thông tin KoiProfile và Category riêng khi cần gửi thông báo
            var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
                .SingleOrDefaultAsync(predicate: k => k.Id == registration.KoiProfileId);
                
            var category = await _unitOfWork.GetRepository<CompetitionCategory>()
                .SingleOrDefaultAsync(predicate: c => c.Id == registration.CompetitionCategoryId);
                
            await _notificationService.SendNotification(registration.AccountId,
                "Cá Koi đã được check-in",
                $"Cá Koi {koiProfile.Name} ở hạng mục {category.Name} của bạn đã được check-in thành công vào triển lãm {show.Name}. Địa điểm check-in: {show.Location}.",
                NotificationType.Registration);
        }

        if (registration.Status == RegistrationStatus.Refunded.ToString().ToLower())
        {
            if (refundType == null)
            {
                throw new BadRequestException("Vui lòng chọn hình thức hoàn tiền");
            }
            registration.RefundType = refundType.ToString().ToLower();
            _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
            await _unitOfWork.CommitAsync();
            
            // Lấy thông tin KoiProfile và Category riêng khi cần gửi thông báo
            var koiProfile = await _unitOfWork.GetRepository<KoiProfile>()
                .SingleOrDefaultAsync(predicate: k => k.Id == registration.KoiProfileId);
                
            var category = await _unitOfWork.GetRepository<CompetitionCategory>()
                .SingleOrDefaultAsync(predicate: c => c.Id == registration.CompetitionCategoryId);
                
            await _notificationService.SendNotification(registration.AccountId,
                "Phí đăng ký cá Koi đã được hoàn tiền",
                $"Phí đăng ký cho cá Koi {koiProfile.Name} ở hạng mục {category.Name} tham gia triển lãm {show.Name} đã được hoàn tiền. Hình thức hoàn tiền: {refundType}. Vui lòng kiểm tra tài khoản của bạn trong vòng 3-5 ngày làm việc. Chi tiết đã được gửi qua email.",
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
            throw new NotFoundException("Không tìm thấy đăng ký");
        }

        if (registration.KoiShow.Status == ShowStatus.Cancelled.ToString().ToLower())
        {
            throw new BadRequestException("Triển lãm đã bị hủy. Không thể thanh toán đăng ký");
        }
        if (registration.AccountId != accountId)
        {
            throw new ForbiddenMethodException("Đây không phải đăng ký của bạn!");
        }
        if (registration.Status == RegistrationStatus.Pending.ToString().ToLower())
        {
            throw new BadRequestException("Đăng ký này đã được thanh toán và đang chờ duyệt!");
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
        var expiryTime = registrationPayment.PaymentDate.AddMinutes(2);

        // Chuyển đổi sang Unix timestamp đảm bảo sử dụng múi giờ Việt Nam (UTC+7)
        // Chỉ định rõ múi giờ UTC+7 khi tạo DateTimeOffset để tránh sai lệch
        var expiredAtTimestamp = new DateTimeOffset(
            expiryTime,
            new TimeSpan(7, 0, 0) // Chỉ định múi giờ Việt Nam (UTC+7)
        ).ToUnixTimeSeconds();
        
        // Sử dụng mô tả ngắn gọn
        var description = $"Đăng ký cá Koi - {registration.KoiShow.Name}";
        
        var paymentData = new PaymentData(
            registrationCode,
            (int)registration.RegistrationFee,
            description,
            items,
            url,
            url,
            expiredAt: expiredAtTimestamp
        );

        var createPayment = await _payOs.createPaymentLink(paymentData);
        registrationPayment.PaymentUrl = createPayment.checkoutUrl;
        _unitOfWork.GetRepository<RegistrationPayment>().UpdateAsync(registrationPayment);
        await _unitOfWork.CommitAsync();
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
                    .Include(r => r.KoiMedia)
                    .Include(r => r.CheckOutLog)
                        .ThenInclude(r => r.CheckedOutByNavigation),
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
        if (filter.Status.Any())
        {
            var statusStrings = filter.Status.Select(s => s.ToString().ToLower()).ToList();
            filterQuery = filterQuery.AndAlso(r => statusStrings.Contains(r.Status));
        }
        if(filter.RegistrationNumber != null)
        {
            filterQuery = filterQuery.AndAlso(r => r.RegistrationNumber == filter.RegistrationNumber);
        }

        return filterQuery;
    }

    private string GetCategoryPrefix(CompetitionCategory category)
    {
        if (string.IsNullOrEmpty(category.Name))
            return "XX";
        var words = category.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (words.Length == 1)
        {
            return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();
        }
        var firstChar = words[0].Length > 0 ? words[0][0].ToString() : "X";
        var lastWord = words[^1];
        var lastChar = lastWord.Length > 0 ? lastWord[0].ToString() : "X";
            
        return (firstChar + lastChar).ToUpper();
    }

    private string GetShowCode(KoiShow show)
    {
        var yearPart = show.StartDate?.Year.ToString() ?? DateTime.Now.Year.ToString();
        var namePart = !string.IsNullOrEmpty(show.Name)
            ? string.Concat(show.Name.Split(' ').Select(s => s.Length > 0 ? s[0] : ' '))
            : "KS";
        namePart = namePart.Length > 3 ? namePart.Substring(0, 3) : namePart;
        return $"{namePart.ToUpper()}{yearPart.Substring(Math.Max(0, yearPart.Length - 2))}";
    }

    public async Task<object> CheckOutRegistrationKoi(Guid registrationId, CreateCheckoutRegistrationKoiRequest request)
    {
        var accountId = GetIdFromJwt();
        var userRole = GetRoleFromJwt();
        
        // Check if the registration exists and is checked in
        var registration = await _unitOfWork.GetRepository<Registration>()
            .SingleOrDefaultAsync(
                predicate: r => r.Id == registrationId,
                include: query => query
                    .Include(r => r.KoiShow)
                    .Include(r => r.KoiProfile));
                    
        if (registration == null)
        {
            throw new NotFoundException("Không tìm thấy đăng ký");
        }
        
        // Verify the user's permission
        if (userRole != "Admin")
        {
            var showStaff = await _unitOfWork.GetRepository<ShowStaff>()
                .SingleOrDefaultAsync(predicate: s => s.AccountId == accountId && s.KoiShowId == registration.KoiShowId);

            if (showStaff is null)
            {
                throw new ForbiddenMethodException("Bạn không có quyền thực hiện check-out cá Koi");
            }
        }
        
        // Check if the registration is in a valid status
        var validStatuses = new[] { 
            RegistrationStatus.CheckIn.ToString().ToLower(), 
            RegistrationStatus.Confirmed.ToString().ToLower(),
            RegistrationStatus.Competition.ToString().ToLower(),
            RegistrationStatus.PrizeWinner.ToString().ToLower(),
            RegistrationStatus.Eliminated.ToString().ToLower()
        };
        
        if (!validStatuses.Contains(registration.Status))
        {
            throw new BadRequestException("Không thể check-out cá Koi với trạng thái đơn đăng ký hiện tại");
        }
        
        // Check if the registration was actually checked in
        if (!registration.IsCheckedIn.HasValue || !registration.IsCheckedIn.Value)
        {
            throw new BadRequestException("Cá Koi chưa được check-in vào triển lãm");
        }
        
        // Check if the Koi has already been checked out
        var existingCheckOut = await _unitOfWork.GetRepository<CheckOutLog>()
            .SingleOrDefaultAsync(predicate: c => c.RegistrationId == registrationId);
            
        if (existingCheckOut != null)
        {
            throw new BadRequestException("Cá Koi này đã được check-out trước đó");
        }
        
        // All checks passed, create the check-out log
        var checkOutLog = new CheckOutLog
        {
            Id = Guid.NewGuid(),
            RegistrationId = registrationId,
            ImgCheckOut = request.ImgCheckOut,
            CheckOutTime = VietNamTimeUtil.GetVietnamTime(),
            CheckedOutBy = accountId,
            Notes = request.Notes
        };
        
        await _unitOfWork.GetRepository<CheckOutLog>().InsertAsync(checkOutLog);
        await _unitOfWork.CommitAsync();
        
        // Send notification to registration owner
        await _notificationService.SendNotification(
            registration.AccountId,
            "Cá Koi đã được check-out",
            $"Cá Koi {registration.KoiProfile.Name} của bạn đã được check-out khỏi triển lãm {registration.KoiShow.Name}",
            NotificationType.Registration);
            
        return new
        {
            Id = checkOutLog.Id,
            Message = "Check-out thành công"
        };
    }
}