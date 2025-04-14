using KSMS.Domain.Dtos.Requests.ShowStatus;

namespace KSMS.Application.Services;

public interface IShowStatusService
{
    Task CreateShowStatusAsync(Guid koiShowId, List<CreateShowStatusRequest> requests);
    
    Task UpdateShowStatusAsync(Guid koiShowId, List<UpdateShowStatusRequestV2> requests);
    
    Task DeleteShowStatusAsync(Guid id);
}