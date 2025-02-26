using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Variety;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class VarietyService : BaseService<VarietyService>, IVarieryService
{
    public VarietyService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<VarietyService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateVariety(CreateVarietyRequest createVarietyRequest)
    {
        await _unitOfWork.GetRepository<Variety>().InsertAsync(createVarietyRequest.Adapt<Variety>());
        await _unitOfWork.CommitAsync();
    }
}