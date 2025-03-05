using KSMS.Domain.Dtos.Requests.Sponsor;

namespace KSMS.Application.Services;

public interface ISponsorService
{
    Task CreateSponsorAsync(Guid koiShowId, CreateSponsorRequest request);
    Task UpdateSponsorAsync(Guid id, UpdateSponsorRequestV2 request);
    Task DeleteSponsorAsync(Guid id);
}