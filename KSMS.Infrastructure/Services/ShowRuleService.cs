using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Responses.ShowRule;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class ShowRuleService : BaseService<ShowRuleService>, IShowRuleService
{
    public ShowRuleService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ShowRuleService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateShowRuleAsync(Guid koiShowId, CreateShowRuleRequest request)
    {
        var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == koiShowId);
        if (show == null)
        {
            throw new NotFoundException("Show not found");
        }
        var rule = request.Adapt<ShowRule>();
        rule.KoiShowId = koiShowId;
        await _unitOfWork.GetRepository<ShowRule>().InsertAsync(rule);
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateShowRuleAsync(Guid id, UpdateShowRuleRequestV2 request)
    {
        var rule = await _unitOfWork.GetRepository<ShowRule>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (rule == null)
        {
            throw new NotFoundException("Show rule not found");
        }
        request.Adapt(rule); 
        _unitOfWork.GetRepository<ShowRule>().UpdateAsync(rule);
        await _unitOfWork.CommitAsync();
    }

    public async Task<Paginate<RuleGetKoiShowDetailResponse>> GetPageShowRuleAsync(Guid showId, int page, int size)
    {
        var rules = await _unitOfWork.GetRepository<ShowRule>()
            .GetPagingListAsync(predicate: x => x.KoiShowId == showId, page: page, size: size);
        return rules.Adapt<Paginate<RuleGetKoiShowDetailResponse>>();
    }

    public async Task DeleteShowRuleAsync(Guid id)
    {
        var rule = await _unitOfWork.GetRepository<ShowRule>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (rule == null)
        {
            throw new NotFoundException("Show rule not found");
        } 
        _unitOfWork.GetRepository<ShowRule>().DeleteAsync(rule);
        await _unitOfWork.CommitAsync();
    }
}