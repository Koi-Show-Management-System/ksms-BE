using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Enums;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IKoiProfileService
{
    Task<GetAllKoiProfileResponse> CreateKoiProfile(CreateKoiProfileRequest createKoiProfileRequest);
    
    Task<Paginate<GetAllKoiProfileResponse>> GetPagedKoiProfile(KoiProfileFilter filter, int page, int size);

    Task UpdateKoiProfile(Guid id, UpdateKoiProfileRequest updateKoiProfileRequest);
    Task UpdateKoiProfileStatus(Guid id, KoiProfileStatus status);

    Task<GetKoiDetailResponse> GetById(Guid id);
}