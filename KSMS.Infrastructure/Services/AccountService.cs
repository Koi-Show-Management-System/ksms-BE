using System.Linq.Expressions;
using KSMS.Application.GoogleServices;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using BCrypt.Net;
using KSMS.Application.Extensions;
using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Hangfire;

namespace KSMS.Infrastructure.Services;

public class AccountService : BaseService<AccountService>, IAccountService
{
    private readonly IFirebaseService _firebaseService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IEmailService _emailService;
    
    public AccountService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<AccountService> logger, 
        IHttpContextAccessor httpContextAccessor, IFirebaseService firebaseService, IBackgroundJobClient backgroundJobClient, IEmailService emailService) 
        : base(unitOfWork, logger, httpContextAccessor)
    {
        _firebaseService = firebaseService;
        _backgroundJobClient = backgroundJobClient;
        _emailService = emailService;
    }
    public async Task<Paginate<AccountResponse>> GetPagedUsersAsync(RoleName? roleName, int page, int size)
    {
        var userRepository = _unitOfWork.GetRepository<Account>();

        Expression<Func<Account, bool>> filterQuery = account => true;
        if (roleName.HasValue)
        {
            var roleString = roleName.Value.ToString();
            filterQuery = filterQuery.AndAlso(r => r.Role == roleString);
        }
        var pagedUsers = await userRepository.GetPagingListAsync(
            predicate: filterQuery,
            orderBy: query => query.OrderBy(a => a.Username),
            page: page,
            size: size
        );
        return pagedUsers.Adapt<Paginate<AccountResponse>>();
    }
    public async Task<AccountResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(
                predicate: u => u.Id == id
            );

        if (user == null)
        {
            throw new NotFoundException("Không tìm thấy người dùng");
        }

        return user.Adapt<AccountResponse>();
    }



    public async Task<AccountResponse> CreateUserAsync(CreateAccountRequest createAccountRequest)
    {
        var userRepository = _unitOfWork.GetRepository<Account>();

        var emailExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Email == createAccountRequest.Email
        );
        if (emailExists != null)
        {
            throw new BadRequestException($"Email '{createAccountRequest.Email}' đã được sử dụng");
        }
        var usernameExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Username == createAccountRequest.Username
        );
        if (usernameExists != null)
        {
            throw new BadRequestException($"Tên đăng nhập '{createAccountRequest.Username}' đã được sử dụng");
        }

        var user = createAccountRequest.Adapt<Account>();
        if (createAccountRequest.AvatarUrl is not null)
        {
            user.Avatar = await _firebaseService.UploadImageAsync(createAccountRequest.AvatarUrl, "account/");
        }
        
        // Tạo mật khẩu ngẫu nhiên cho tài khoản
        string randomPassword = GenerateRandomPassword();
        user.HashedPassword = PasswordUtil.HashPassword(randomPassword);
        
        user.IsConfirmed = true;
        var createdUser = await userRepository.InsertAsync(user);
        await _unitOfWork.CommitAsync();
        _backgroundJobClient.Enqueue(() => 
            _emailService.SendNewAccountNotificationEmail(createdUser.Id, randomPassword));
        
        return createdUser.Adapt<AccountResponse>();
    }

    public async Task<AccountResponse> UpdateStatus(Guid id, AccountStatus status)
    {
        var userRepository = _unitOfWork.GetRepository<Account>();

        var user = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Id == id
        );

        if (user == null)
            throw new NotFoundException("Không tìm thấy người dùng");

        user.Status = status switch
        {
            AccountStatus.Blocked => AccountStatus.Blocked.ToString().ToLower(),
            AccountStatus.Deleted => AccountStatus.Deleted.ToString().ToLower(),
            AccountStatus.Active => AccountStatus.Active.ToString().ToLower(),
            _ => user.Status
        };
        userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
        return user.Adapt<AccountResponse>();
    }

    public async Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate: a => a.Id == id);
        if (account == null)
        {
            throw new NotFoundException("Không tìm thấy người dùng");
        }
        var existingAccount = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: a => a.Username.ToLower() == updateAccountRequest.Username.ToLower() && a.Id != id);
        if (existingAccount != null)
        {
            throw new Exception("Tên người dùng này đã tồn tại");
        } 
        updateAccountRequest.Adapt(account);
        if (account.Avatar is not null)
        {
            await _firebaseService.DeleteImageAsync(account.Avatar);
            account.Avatar = null;
        }

        if (updateAccountRequest.AvatarUrl != null)
        {
            account.Avatar = await _firebaseService.UploadImageAsync(updateAccountRequest.AvatarUrl, "account/");
        }
        _unitOfWork.GetRepository<Account>().UpdateAsync(account);
        await _unitOfWork.CommitAsync();
    }

    private string GenerateRandomPassword(int length = 12)
    {
        const string upperChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijkmnopqrstuvwxyz";
        const string numericChars = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        
        var random = new Random();
        var password = new List<char>();
        
        // Bắt buộc có ít nhất 1 ký tự từ mỗi loại
        password.Add(upperChars[random.Next(upperChars.Length)]);
        password.Add(lowerChars[random.Next(lowerChars.Length)]);
        password.Add(numericChars[random.Next(numericChars.Length)]);
        password.Add(specialChars[random.Next(specialChars.Length)]);
        
        // Tạo các ký tự còn lại ngẫu nhiên
        var allChars = upperChars + lowerChars + numericChars + specialChars;
        var remainingLength = length - 4;
        
        for (var i = 0; i < remainingLength; i++)
        {
            password.Add(allChars[random.Next(allChars.Length)]);
        }
        
        // Xáo trộn mật khẩu
        for (var i = password.Count - 1; i > 0; i--)
        {
            var swapIndex = random.Next(i + 1);
            (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
        }
        
        return new string(password.ToArray());
    }
}