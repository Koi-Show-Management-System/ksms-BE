using KSMS.Application.Extensions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Common;
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
    private const int OTP_EXPIRY_MINUTES = 5;
    
    public AuthenticationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<AuthenticationService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }
    public async Task Register(RegisterRequest registerRequest)
    {
        var accountDb = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: x => x.Email == registerRequest.Email);
        if (accountDb is not null)
        {
            throw new BadRequestException("Email này đã tồn tại");
        }
        var accountDb1 = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate:x => x.Username == registerRequest.Username);
        if (accountDb1 is not null)
        {
            throw new BadRequestException("Tên đăng nhập này đã tồn tại");
        }
        var account = registerRequest.Adapt<Account>();
        account.Role =RoleName.Member.ToString();
        account.HashedPassword = PasswordUtil.HashPassword(registerRequest.Password);
        account.ConfirmationToken = Guid.NewGuid().ToString();
        account.Status = AccountStatus.Active.GetDescription().ToLower();
        await _unitOfWork.GetRepository<Account>().InsertAsync(account);
        await _unitOfWork.CommitAsync();
        var confirmationLink = $"{AppConfig.AppSetting.BaseUrl}/swagger/confirm?token={account.ConfirmationToken}";
        if (!MailUtil.SendEmail(registerRequest.Email, MailUtil.ContentMailUtil.Title_ThankingForRegisAccount,
                MailUtil.ContentMailUtil.ThankingForRegistration(registerRequest.FullName, confirmationLink), ""))
        {
            throw new BadRequestException("Lỗi khi gửi email xác nhận.");
        }
        
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate: a =>
                a.Email == loginRequest.Email && a.HashedPassword == PasswordUtil.HashPassword(loginRequest.Password));
        if (account is null)
        {
            throw new NotFoundException("Sai email hoặc mật khẩu!");
        }
    
        if (account.Status == AccountStatus.Deleted.ToString().ToLower())
        {
            throw new BadRequestException("Tài khoản không tồn tại");
        }
        if (account.Status == AccountStatus.Blocked.ToString().ToLower())
        {
            throw new BadRequestException("Tài khoản đã bị khóa");
        }

        if (account.IsConfirmed == false)
        {
            throw new BadRequestException("Vui lòng xác nhận qua đường dẫn trong email của bạn. Nếu không thấy, hãy kiểm tra thư mục Spam");
        }
        return new LoginResponse()
        {
            Id = account.Id,
            Email = account.Email,
            FullName = account.FullName,
            Token = JwtUtil.GenerateJwtToken(account),
            Role = account.Role
        };

    }

    public async Task SendForgotPasswordOTP(string email)
    {
        var account = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: a => a.Email == email);
            
        if (account == null)
            throw new NotFoundException("Email không tồn tại.");
        if (account.ResetPasswordOtp != null && account.ResetPasswordOtpexpiry > VietNamTimeUtil.GetVietnamTime())
        {
            var remainingTime = (account.ResetPasswordOtpexpiry.Value - VietNamTimeUtil.GetVietnamTime()).TotalSeconds;
            throw new BadRequestException($"Vui lòng đợi {(int)remainingTime} giây trước khi yêu cầu mã OTP mới.");
        }
        var otp = Random.Shared.Next(100000, 999999).ToString();
        account.ResetPasswordOtp = otp;
        account.ResetPasswordOtpexpiry = VietNamTimeUtil.GetVietnamTime().AddMinutes(OTP_EXPIRY_MINUTES);
        
        _unitOfWork.GetRepository<Account>().UpdateAsync(account);
        await _unitOfWork.CommitAsync();
        var sendMail = MailUtil.SendEmail(
            email,
            MailUtil.ContentMailUtil.Title_ForgotPassword,
            MailUtil.ContentMailUtil.ForgotPasswordOTP(account.FullName, otp),
            ""
        );

        if (!sendMail)
            throw new BadRequestException("Không thể gửi email OTP.");
    }

    public async Task ResetPassword(string email, string otp, string newPassword)
    {
        var account = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(predicate: a => a.Email == email);
            
        if (account == null)
            throw new NotFoundException("Email không tồn tại.");
        
        if (account.ResetPasswordOtp != otp)
            throw new BadRequestException("Mã OTP không chính xác.");
        
        if (account.ResetPasswordOtpexpiry < VietNamTimeUtil.GetVietnamTime())
            throw new BadRequestException("Mã OTP đã hết hạn.");

        // Cập nhật mật khẩu mới
        account.HashedPassword = PasswordUtil.HashPassword(newPassword);
        account.ResetPasswordOtp = null;
        account.ResetPasswordOtpexpiry = null;

        _unitOfWork.GetRepository<Account>().UpdateAsync(account);
        await _unitOfWork.CommitAsync();
    }
}