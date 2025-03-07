using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Responses.ShowRule;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IShowRuleService
{
    Task CreateShowRuleAsync(Guid koiShowId, CreateShowRuleRequest request);
    
    Task UpdateShowRuleAsync(Guid id, UpdateShowRuleRequestV2 request);
    
    Task<Paginate<RuleGetKoiShowDetailResponse>> GetPageShowRuleAsync(Guid showId, int page, int size);
    
    Task DeleteShowRuleAsync(Guid id);
}