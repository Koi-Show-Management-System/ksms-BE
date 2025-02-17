using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Authentication;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class AuthenticationService : BaseService<AuthenticationService>, IAuthenticationService
{
    public AuthenticationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<AuthenticationService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }
    public async Task Register(RegisterRequest registerRequest)
    {
        var accountDb = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: x => x.Email == registerRequest.Email);
        if (accountDb is not null)
        {
            throw new BadRequestException("Email is already existed");
        }
        var account = registerRequest.Adapt<Account>();
        account.Role =RoleName.Member.ToString();
        account.HashedPassword = PasswordUtil.HashPassword(registerRequest.Password);
        account.ConfirmationToken = Guid.NewGuid().ToString();
        await _unitOfWork.GetRepository<Account>().InsertAsync(account);
        var confirmationLink = $"https://localhost:7042/swagger/confirm?token={account.ConfirmationToken}";
        if (!MailUtil.SendEmail(registerRequest.Email, MailUtil.ContentMailUtil.Title_ThankingForRegisAccount,
                MailUtil.ContentMailUtil.ThankingForRegistration(registerRequest.FullName, confirmationLink), ""))
        {
            throw new BadRequestException("Error sending confirmation email.");
        }
        await _unitOfWork.CommitAsync();
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate: a =>
                a.Email == loginRequest.Email && a.HashedPassword == PasswordUtil.HashPassword(loginRequest.Password));
        if (account is null)
        {
            throw new NotFoundException("Wrong email or password!!!");
        }
    
        if (account.Status == AccountStatus.Deleted.ToString().ToLower())
        {
            throw new BadRequestException("Account is not existed");
        }
        if (account.Status == AccountStatus.Blocked.ToString().ToLower())
        {
            throw new BadRequestException("Account is blocked");
        }

        if (account.IsConfirmed == false)
        {
            throw new BadRequestException("Please confirm via link in your email box. If you don't see please check the folder Spam in Mail");
        }
        return new LoginResponse()
        {
            Id = account.Id,
            Email = account.Email,
            Token = JwtUtil.GenerateJwtToken(account),
            Role = account.Role
        };

    }

    
}