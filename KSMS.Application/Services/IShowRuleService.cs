using KSMS.Domain.Dtos.Requests.ShowRule;

namespace KSMS.Application.Services;

public interface IShowRuleService
{
    Task CreateShowRuleAsync(Guid koiShowId, CreateShowRuleRequest request);
    
    Task UpdateShowRuleAsync(Guid id, UpdateShowRuleRequestV2 request);
    
    Task DeleteShowRuleAsync(Guid id);
}