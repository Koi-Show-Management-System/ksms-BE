using System.Net;
using System.Security.Claims;
using KSMS.Application.Repositories;
using KSMS.Domain.Exceptions;
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
    protected IHttpContextAccessor _httpContextAccessor;

    public BaseService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<T> logger, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    protected Guid GetIdFromJwt()
    {
        var idClaim = _httpContextAccessor?.HttpContext?.User
            .FindFirst("Id")?.Value;
    
        if (string.IsNullOrEmpty(idClaim))
        {
            throw new UnauthorizedException("User ID not found in token");
        }

        if (!Guid.TryParse(idClaim, out var accountId))
        {
            throw new UnauthorizedException("Invalid User ID format");
        }

        return accountId;
    }
}