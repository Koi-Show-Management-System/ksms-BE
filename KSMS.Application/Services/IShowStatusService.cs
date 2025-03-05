using KSMS.Domain.Dtos.Requests.ShowStatus;

namespace KSMS.Application.Services;

public interface IShowStatusService
{
    Task CreateShowStatusAsync(Guid koiShowId, CreateShowStatusRequest request);
    
    Task UpdateShowStatusAsync(Guid id, UpdateShowStatusRequestV2 request);
    
    Task DeleteShowStatusAsync(Guid id);
}