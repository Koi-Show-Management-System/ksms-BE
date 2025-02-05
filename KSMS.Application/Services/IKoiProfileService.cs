using System.Security.Claims;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IKoiProfileService
{
    Task<object> CreateKoiProfile(ClaimsPrincipal claimsPrincipal, CreateKoiProfileRequest createKoiProfileRequest);
    
    Task<Paginate<GetAllKoiProfileResponse>> GetPagedKoiProfile(ClaimsPrincipal claims, KoiProfileFilter filter, int page, int size);

    Task<object> UpdateKoiProfile(ClaimsPrincipal claims, Guid id, UpdateKoiProfileRequest updateKoiProfileRequest);
}