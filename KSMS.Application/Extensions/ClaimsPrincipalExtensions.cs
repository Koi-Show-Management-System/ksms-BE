using System.Security.Claims;
using KSMS.Domain.Exceptions;

namespace KSMS.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAccountId(this ClaimsPrincipal claims)
    {
        var accountId = claims.FindFirst(c => c.Type == "Id")?.Value;

        return (accountId is not null) ? Guid.Parse(accountId) : throw new UnauthorizedException("Invalid claims principal");
    }
}