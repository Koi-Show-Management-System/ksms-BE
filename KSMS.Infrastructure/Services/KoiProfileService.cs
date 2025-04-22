using System.Linq.Expressions;
using System.Security.Claims;
using KSMS.Application.Extensions;
using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Infrastructure.Services;

public class KoiProfileService : BaseService<KoiProfileService>, IKoiProfileService
{
    private readonly IFirebaseService _firebaseService;
    private readonly IMediaService _mediaService;
   
    public KoiProfileService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<KoiProfileService> logger, IHttpContextAccessor httpContextAccessor, IFirebaseService firebaseService, IMediaService mediaService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _firebaseService = firebaseService;
        _mediaService = mediaService;
    }
    public async Task<GetAllKoiProfileResponse> CreateKoiProfile(CreateKoiProfileRequest createKoiProfileRequest)
    {
        var accountId = GetIdFromJwt();
        var variety = await _unitOfWork.GetRepository<Variety>()
            .SingleOrDefaultAsync(predicate: v => v.Id == createKoiProfileRequest.VarietyId);
        if (variety is null)
        {
            throw new NotFoundException("Không tìm thấy giống cá");
        }
        var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
            k.Name.ToLower() == createKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);
        if (koi is not null)
        {
            throw new BadRequestException("Cá Koi này đã có trong danh sách của bạn");
        }

        var koiProfile = createKoiProfileRequest.Adapt<KoiProfile>();
        koiProfile.OwnerId = accountId;
        await _unitOfWork.GetRepository<KoiProfile>().InsertAsync(koiProfile);
        await _unitOfWork.CommitAsync();
        if (createKoiProfileRequest.KoiImages is not [])
        {
            await _mediaService.UploadKoiImage(createKoiProfileRequest.KoiImages,
                koiProfile.Id);
        }

        if (createKoiProfileRequest.KoiVideos is not [])
        {
            await _mediaService.UploadKoiVideos(createKoiProfileRequest.KoiVideos,
                koiProfile.Id);
        }

        var koiDb = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(
            predicate: k => k.Id == koiProfile.Id, include:
            query => query.Include(k => k.Variety).Include(k => k.KoiMedia));
        return koiDb.Adapt<GetAllKoiProfileResponse>();
    }

    public async Task<Paginate<GetAllKoiProfileResponse>> GetPagedKoiProfile(KoiProfileFilter filter, int page, int size)
    {
        var accountId = GetIdFromJwt();
        var listKoi = await _unitOfWork.GetRepository<KoiProfile>().GetPagingListAsync(predicate: ApplyKoiFilter(filter, accountId),
            page: page, size: size, include: query => query.Include(k => k.Variety)
                .Include(k => k.KoiMedia));
        return listKoi.Adapt<Paginate<GetAllKoiProfileResponse>>();
    }

    public async Task UpdateKoiProfile(Guid id, UpdateKoiProfileRequest updateKoiProfileRequest)
    {
        var accountId = GetIdFromJwt();
        var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k => k.Id == id,
            include: query => query.Include(r => r.KoiMedia));
        if (koi is null)
        {
            throw new NotFoundException("Không tìm thấy cá Koi");
        }

        if (koi.OwnerId != accountId)
        {
            throw new ForbiddenMethodException("Đây không phải cá Koi của bạn!");
        }
        
        if (updateKoiProfileRequest.Name is not null)
        {
            var existingKoi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
                k.Name.ToLower() == updateKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);

            if (existingKoi is not null && existingKoi.Id != koi.Id)
            {
                throw new BadRequestException("Tên này đã tồn tại. Vui lòng chọn tên khác");
            }
        }
        var waitToPaidRegistrations = await _unitOfWork.GetRepository<Registration>()
            .GetListAsync(predicate: r => r.KoiProfileId == koi.Id 
                                          && r.Status == RegistrationStatus.WaitToPaid.ToString().ToLower());
        if (waitToPaidRegistrations.Any())
        {
            throw new BadRequestException("Bạn cần phải hủy tất cả các đơn đăng ký đang chờ thanh toán trước khi thay đổi thông tin cá Koi");
        }
        var activeRegistrations = await _unitOfWork.GetRepository<Registration>().GetListAsync(
            predicate: r => r.KoiProfileId == id && 
                            r.KoiShow.Status != ShowStatus.Finished.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Cancelled.ToString().ToLower() &&
                            r.Status != RegistrationStatus.PendingRefund.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Rejected.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Refunded.ToString().ToLower(),
            include: query => query.Include(r => r.KoiShow));
        if (activeRegistrations.Any())
        {
            throw new BadRequestException("Không thể cập nhật thông tin cá Koi đang tham gia cuộc thi. Vui lòng đợi cuộc thi kết thúc");
        }
        if (updateKoiProfileRequest.VarietyId is not null)
        {
            var variety = await _unitOfWork.GetRepository<Variety>()
                .SingleOrDefaultAsync(predicate: v => v.Id == updateKoiProfileRequest.VarietyId);
            if (variety is null)
            {
                throw new NotFoundException("Không tìm thấy giống cá");
            }
        }
        updateKoiProfileRequest.Adapt(koi);
        _unitOfWork.GetRepository<KoiProfile>().UpdateAsync(koi);
        await _unitOfWork.CommitAsync();
        if (koi.KoiMedia.Any())
        {
            await _mediaService.DeleteFiles(koi.KoiMedia);
            koi.KoiMedia.Clear();
        }

        await _unitOfWork.CommitAsync();
        if (updateKoiProfileRequest.KoiImages is not [])
        {
            await _mediaService.UploadKoiImage(updateKoiProfileRequest.KoiImages,
                koi.Id);
        }

        if (updateKoiProfileRequest.KoiVideos is not [])
        {
            await _mediaService.UploadKoiVideos(updateKoiProfileRequest.KoiVideos,
                koi.Id);
        }
    }

    public async Task<GetKoiDetailResponse> GetById(Guid id)
    {
        var koiProfile = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: x => x.Id == id,
            include: query => query.AsSplitQuery()
                .Include(k => k.Variety)
                .Include(k => k.KoiMedia)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.KoiShow)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.CompetitionCategory)
                        .ThenInclude(s => s.Awards)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.RoundResults)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.Votes));
        if (koiProfile is null)
        {
            throw new NotFoundException("Không tìm thấy cá Koi");
        }
        
        var response = koiProfile.Adapt<GetKoiDetailResponse>();
        // var validRegistrations = koiProfile.Registrations.Where(r =>
        //     r.Status != RegistrationStatus.WaitToPaid.ToString().ToLower() &&
        //     r.Status != RegistrationStatus.Cancelled.ToString().ToLower() &&
        //     r.Status != RegistrationStatus.Pending.ToString().ToLower() &&
        //     r.Status != RegistrationStatus.Rejected.ToString().ToLower()
        // ).ToList();
        var prizeWinnerRegistrations = koiProfile.Registrations.Where(r =>
            r.Status == "prizewinner").ToList();
        foreach (var registration in prizeWinnerRegistrations)
        {
            var award = registration.CompetitionCategory.Awards
                .FirstOrDefault(a => GetAwardNameByRank(registration.Rank.Value) == a.AwardType);
            if (award != null)
            {
                response.Achievements.Add(new KoiAchievementResponse
                {
                    ShowName = registration.KoiShow.Name,
                    Location = registration.KoiShow.Location,
                    CategoryName = registration.CompetitionCategory.Name,
                    AwardType = award.AwardType,
                    PrizeValue = award.PrizeValue,
                    AwardName = award.Name,
                    CompetitionDate = registration.KoiShow.EndDate
                });
            }
        }

        var showIds = koiProfile.Registrations
            .Where(r => !r.KoiShow.EnableVoting && r.Votes.Any())
            .Select(r => r.KoiShowId)
            .Distinct()
            .ToList();
        var showMaxVotes = new Dictionary<Guid, int>();
        foreach (var showId in showIds)
        {
            var votes = await _unitOfWork.GetRepository<Registration>()
                .GetListAsync(
                    selector: r => r.Votes.Count,
                    predicate: r => r.KoiShowId == showId);
            showMaxVotes[showId] = votes.Max();
        }
        foreach (var registration in koiProfile.Registrations)
        {
            if (!registration.KoiShow.EnableVoting &&
                registration.Votes.Any() &&
                showMaxVotes.TryGetValue(registration.KoiShowId, out int maxVotes) &&
                registration.Votes.Count == maxVotes)
            {
                response.Achievements.Add(new KoiAchievementResponse
                {
                    ShowName = registration.KoiShow.Name,
                    Location = registration.KoiShow.Location,
                    CategoryName = null,
                    AwardType = "peoples_choice",
                    PrizeValue = null,
                    AwardName = "Giải bình chọn khán giả",
                    CompetitionDate = registration.KoiShow.EndDate
                });
               
            }
        }
        response.Achievements = response.Achievements
            .OrderByDescending(a => a.CompetitionDate)
            .ToList();
        response.CompetitionHistory = koiProfile.Registrations
            .OrderByDescending(r => r.KoiShow.EndDate)
            .Select(r => new KoiCompetitionHistoryResponse
            {
                KoiShowId = r.KoiShowId,
                Year = r.KoiShow.EndDate?.Year.ToString(),
                ShowName = r.KoiShow.Name,
                ShowStatus = r.KoiShow.Status,
                Location = r.KoiShow.Location ?? "",
                Result = GetCompetitionResult(r),
                EliminationRound = GetEliminationRoundInfo(r)
                
            }).ToList();
        return response;

    }

    private string? GetAwardNameByRank(int rank)
    {
        return rank switch
        {
            1 => "first",
            2 => "second",
            3 => "third",
            4 => "honorable",
            _ => null
        };
    }

    private string GetCompetitionResult(Registration registration)
    {
        if (registration == null)
            return "Không có thông tin";
        
        // Get category name for display
        string categoryInfo = !string.IsNullOrEmpty(registration.CompetitionCategory?.Name) 
            ? $" (Hạng mục: {registration.CompetitionCategory.Name})" 
            : "";
        
        // TRƯỜNG HỢP ĐẶC BIỆT: Nếu show đã kết thúc và registration ở trạng thái confirmed
        if (registration.KoiShow?.Status == ShowStatus.Finished.ToString().ToLower() && 
            registration.Status?.ToLower() == RegistrationStatus.Confirmed.ToString().ToLower())
        {
            return $"Không tham gia thi đấu (không check in){categoryInfo}";
        }
        
        string status = registration.Status?.ToLower() ?? string.Empty;
        
        // XỬ LÝ THEO STATUS
        switch (status)
        {
            // TRƯỜNG HỢP CÁ ĐÃ ĐẠT GIẢI
            case var s when s == RegistrationStatus.PrizeWinner.ToString().ToLower():
                if (!registration.Rank.HasValue)
                    return $"Đạt giải thưởng{categoryInfo}";
                    
                return registration.Rank.Value switch
                {
                    1 => $"Giải nhất{categoryInfo}",
                    2 => $"Giải nhì{categoryInfo}",
                    3 => $"Giải ba{categoryInfo}",
                    4 => $"Giải khuyến khích{categoryInfo}",
                    _ => $"Giải thưởng (Top {registration.Rank.Value}){categoryInfo}"
                };
                
            // TRƯỜNG HỢP CÁ ĐÃ BỊ LOẠI
            case var s when s == RegistrationStatus.Eliminated.ToString().ToLower():
                return registration.Rank.HasValue
                    ? $"Thứ hạng sau cùng: {registration.Rank.Value} - Đã bị loại{categoryInfo}"
                    : $"Đã bị loại{categoryInfo}";
                    
            // TRƯỜNG HỢP CÁ ĐANG THI ĐẤU
            case var s when s == RegistrationStatus.Competition.ToString().ToLower():
                return registration.Rank.HasValue
                    ? $"Thứ hạng hiện tại: {registration.Rank.Value} - Đang thi đấu{categoryInfo}"
                    : $"Đang thi đấu{categoryInfo}";
                    
            // CÁC TRẠNG THÁI KHÁC KHÔNG CÓ RANK
            case var s when s == RegistrationStatus.WaitToPaid.ToString().ToLower():
                return $"Chờ thanh toán{categoryInfo}";
                
            case var s when s == RegistrationStatus.Pending.ToString().ToLower():
                return $"Chờ duyệt{categoryInfo}";
                
            case var s when s == RegistrationStatus.Rejected.ToString().ToLower():
                return $"Bị từ chối{categoryInfo}";
                
            case var s when s == RegistrationStatus.Cancelled.ToString().ToLower():
                return $"Đã hủy{categoryInfo}";
                
            case var s when s == RegistrationStatus.Confirmed.ToString().ToLower():
                return $"Đã được duyệt - Chờ check-in{categoryInfo}";
                
            case var s when s == RegistrationStatus.CheckIn.ToString().ToLower():
                return $"Đã check-in và đang chờ thi đấu{categoryInfo}";
                
            case var s when s == RegistrationStatus.PendingRefund.ToString().ToLower():
                return $"Chờ hoàn tiền{categoryInfo}";
                
            case var s when s == RegistrationStatus.Refunded.ToString().ToLower():
                return $"Đã hoàn tiền{categoryInfo}";
                
            case var s when s == RegistrationStatus.CheckedOut.ToString().ToLower():
                return $"Đã check-out khỏi cuộc thi{categoryInfo}";
                
            default:
                return registration.Rank.HasValue
                    ? $"Thứ hạng: {registration.Rank.Value}{categoryInfo}"
                    : $"Trạng thái không xác định{categoryInfo}";
        }
    }

    private string? GetEliminationRoundInfo(Registration registration)
    {
        if (registration.Status == "eliminated" && registration.RegistrationRounds.Any())
        {
            var failedRound = registration.RegistrationRounds
                .Where(rr => rr.RoundResults.Any(result => result.Status == "Fail"))
                .OrderBy(rr => GetRoundTypeOrder(rr.Round.RoundType))
                .ThenBy(rr => rr.Round.RoundOrder)
                .FirstOrDefault();
            if (failedRound?.Round != null)
            {
                string roundType = failedRound.Round.RoundType;
                string roundName = !string.IsNullOrEmpty(failedRound.Round.Name) 
                    ? failedRound.Round.Name 
                    : (failedRound.Round.RoundOrder.HasValue ? $"Vòng {failedRound.Round.RoundOrder}" : "");
                
                return roundType switch
                {
                    var type when type.Equals(RoundEnum.Preliminary.ToString(), StringComparison.OrdinalIgnoreCase) => $"Vòng sơ khảo-{roundName}",
                    var type when type.Equals(RoundEnum.Evaluation.ToString(), StringComparison.OrdinalIgnoreCase) => $"Vòng đánh giá-{roundName}",
                    var type when type.Equals(RoundEnum.Final.ToString(), StringComparison.OrdinalIgnoreCase) => $"Vòng chung kết-{roundName}",
                    _ => $"Vòng {roundType} {roundName}"
                };
            }
        }

        return null;
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
    private Expression<Func<KoiProfile, bool>> ApplyKoiFilter(KoiProfileFilter? filter, Guid accountId)
    {
        if (filter == null) return koi => koi.OwnerId == accountId;

        Expression<Func<KoiProfile, bool>> filterQuery = koi => koi.OwnerId == accountId;
        if (filter.StartSize < filter.EndSize)
        {
            filterQuery = filterQuery.AndAlso(koi => koi.Size >= filter.StartSize && koi.Size <= filter.EndSize);
        }
        if (filter.VarietyIds is not [])
        {
            filterQuery = filterQuery.AndAlso(koi => filter.VarietyIds.Contains(koi.VarietyId));
        }
        if (!string.IsNullOrEmpty(filter.Name))
        {
            filterQuery = filterQuery.AndAlso(koi => !string.IsNullOrEmpty(koi.Name) &&
                                                     koi.Name.ToLower().Contains(filter.Name.ToLower()));
        }
        return filterQuery;
    }
    
    public async Task UpdateKoiProfileStatus(Guid id, KoiProfileStatus status)
    {
        var accountId = GetIdFromJwt();
        var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(
            predicate: k => k.Id == id);
            
        if (koi is null)
        {
            throw new NotFoundException("Không tìm thấy cá Koi");
        }

        if (koi.OwnerId != accountId)
        {
            throw new ForbiddenMethodException("Đây không phải cá Koi của bạn!");
        }
        var activeRegistrations = await _unitOfWork.GetRepository<Registration>().GetListAsync(
            predicate: r => r.KoiProfileId == id && 
                            r.KoiShow.Status != ShowStatus.Finished.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Cancelled.ToString().ToLower() &&
                            r.Status != RegistrationStatus.PendingRefund.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Rejected.ToString().ToLower() &&
                            r.Status != RegistrationStatus.Refunded.ToString().ToLower(),
            include: query => query.Include(r => r.KoiShow));
            
        if (activeRegistrations.Any() && status ==  KoiProfileStatus.Inactive)
        {
            throw new BadRequestException("Không thể vô hiệu hóa cá Koi đang tham gia cuộc thi. Vui lòng đợi cuộc thi kết thúc");
        }
        koi.Status = status switch
        {
            KoiProfileStatus.Active => KoiProfileStatus.Active.ToString().ToLower(),
            KoiProfileStatus.Inactive => KoiProfileStatus.Inactive.ToString().ToLower(),
            _ => koi.Status
        };
        _unitOfWork.GetRepository<KoiProfile>().UpdateAsync(koi);
        await _unitOfWork.CommitAsync();
    }
}