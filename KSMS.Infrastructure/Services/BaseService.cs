using System.Net;
using System.Security.Claims;
using KSMS.Application.Repositories;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public abstract class BaseService<T> where T : class
{
    protected IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;
    protected ILogger<T> _logger;

    public BaseService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<T> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
}