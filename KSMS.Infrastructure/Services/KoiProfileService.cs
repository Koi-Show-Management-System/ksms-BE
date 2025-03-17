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
            throw new NotFoundException("Variety is not found");
        }
        var koi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
            k.Name.ToLower() == createKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);
        if (koi is not null)
        {
            throw new BadRequestException("Koi is already add to your list");
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
            throw new NotFoundException("Koi is not existed");
        }

        if (koi.OwnerId != accountId)
        {
            throw new ForbiddenMethodException("This koi is not yours!!!!!");
        }
        
        if (updateKoiProfileRequest.Name is not null)
        {
            var existingKoi = await _unitOfWork.GetRepository<KoiProfile>().SingleOrDefaultAsync(predicate: k =>
                k.Name.ToLower() == updateKoiProfileRequest.Name.ToLower() && k.OwnerId == accountId);

            if (existingKoi is not null && existingKoi.Id != koi.Id)
            {
                throw new BadRequestException("Name is already existed. Please choose another name");
            }
        }
        if (updateKoiProfileRequest.VarietyId is not null)
        {
            var variety = await _unitOfWork.GetRepository<Variety>()
                .SingleOrDefaultAsync(predicate: v => v.Id == updateKoiProfileRequest.VarietyId);
            if (variety is null)
            {
                throw new NotFoundException("Variety is not found");
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
            include: query => query
                .Include(k => k.Variety)
                .Include(k => k.KoiMedia)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.KoiShow)
                .Include(k => k.Registrations)
                    .ThenInclude(r => r.CompetitionCategory)
                        .ThenInclude(s => s.Awards));
        if (koiProfile is null)
        {
            throw new NotFoundException("Koi is not found");
        }
        
        var response = koiProfile.Adapt<GetKoiDetailResponse>();
        var validRegistrations = koiProfile.Registrations.Where(r =>
            r.Status != RegistrationStatus.WaitToPaid.ToString().ToLower() &&
            r.Status != RegistrationStatus.Cancelled.ToString().ToLower() &&
            r.Status != RegistrationStatus.Pending.ToString().ToLower() &&
            r.Status != RegistrationStatus.Rejected.ToString().ToLower()
        ).ToList();
        foreach (var registration in validRegistrations.Where(
                     r => r.Rank.HasValue && r.KoiShow.Status == ShowStatus.Finished.ToString().ToLower()))
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

        response.CompetitionHistory = validRegistrations
            .OrderByDescending(r => r.KoiShow.EndDate)
            .Select(r => new KoiCompetitionHistoryResponse
            {
                Year = r.KoiShow.EndDate?.Year.ToString(),
                ShowName = r.KoiShow.Name,
                ShowStatus = r.KoiShow.Status,
                Location = r.KoiShow.Location ?? "",
                Result = GetCompetitionResult(r)
            }).ToList();
        return response;

    }

    private string GetAwardNameByRank(int rank)
    {
        return rank switch
        {
            1 => "Giải nhất",
            2 => "Giải nhì",
            3 => "Giải ba",
            _ => "Participation"
        };
    }

    private string GetCompetitionResult(Registration registration)
    {
       
        if (registration.KoiShow.Status == ShowStatus.Finished.ToString().ToLower())
        {
            if (!registration.Rank.HasValue)
                return "Không tham gia thi đấu (không check in)";
            return registration.Rank.Value switch
            {
                1 => "Giải nhất",
                2 => "Giải nhì",
                3 => "Giải ba",
                _ => $"Top {registration.Rank.Value}th"
            };
        }

        if (!registration.Rank.HasValue)
        {
            if (registration.Status == RegistrationStatus.Confirmed.ToString().ToLower())
            {
                return "Đã được duyệt - Chờ check in";
            }
            if (registration.Status == RegistrationStatus.CheckIn.ToString().ToLower())
            {
                return "Đã check in và đang chờ thi đấu";
            }

            return "Đang thi đấu";
        }

        return $"Thứ hạng hiện tại: {registration.Rank.Value}";
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
    
    
}