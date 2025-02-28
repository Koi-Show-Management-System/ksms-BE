using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.ErrorType;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class ErrorTypeService : BaseService<ErrorType>, IErrorTypeService
{
    public ErrorTypeService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ErrorType> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task<object> CreateErrorType(CreateErrorTypeRequest request)
    {
        var criteria = await _unitOfWork.GetRepository<Criterion>()
            .SingleOrDefaultAsync(predicate: c => c.Id == request.CriteriaId);
        if (criteria is null)
        {
            throw new NotFoundException("Criteria is not existed");
        }

        var errorType = request.Adapt<Criterion>();
        await _unitOfWork.GetRepository<Criterion>().InsertAsync(errorType);
        await _unitOfWork.CommitAsync();
        return new
        {
            Id = errorType.Id
        };
    }
}