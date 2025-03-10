using KSMS.Domain.Dtos.Requests.Sponsor;
using KSMS.Domain.Dtos.Responses.Sponsor;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface ISponsorService
{
    Task CreateSponsorAsync(Guid koiShowId, CreateSponsorRequest request);
    Task UpdateSponsorAsync(Guid id, UpdateSponsorRequestV2 request);
    Task DeleteSponsorAsync(Guid id);
    
    Task<Paginate<SponsorGetKoiShowDetailResponse>> GetPageSponsorAsync(Guid koiShowId, int page, int size);
}