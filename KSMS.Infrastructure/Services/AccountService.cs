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

namespace KSMS.Infrastructure.Services;

public class AccountService : BaseService<AccountService>, IAccountService
{
    private readonly IFirebaseService _firebaseService;
    public AccountService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<AccountService> logger, IFirebaseService firebaseService) : base(unitOfWork, logger)
    {
        _firebaseService = firebaseService;
    }
    public async Task<IPaginate<UserResponse>> GetPagedUsersAsync(int page, int size)
    {
        
        var userRepository = _unitOfWork.GetRepository<Account>();

       
        var pagedUsers = await userRepository.GetPagingListAsync(
            selector: account => new UserResponse
            {
                Id = account.Id,
                Email = account.Email,
                FullName = account.FullName,
                Phone = account.Phone,
                Role = account.Role != null ? account.Role.Name : RoleName.Member.ToString(),
                Status = account.Status ?? "active",
                RoleId = account.RoleId,
                Avatar = account.Avatar
            },
            predicate: null,  
            orderBy: query => query.OrderBy(a => a.Username),  
            include: query => query.Include(a => a.Role),  
            page: page,  
            size: size   
        );

        return pagedUsers;
    }
    public async Task<UserResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(
                predicate: u => u.Id == id,
                include: query => query.Include(u => u.Role) 
            );

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role != null ? user.Role.Name : "N/A",
            RoleId = user.RoleId,
            Status = user.Status ?? "active",
            Avatar = user.Avatar,
            ConfirmationToken = user.ConfirmationToken,
            IsConfirmed = user.IsConfirmed
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateAccountRequest createAccountRequest)
    {
         
        var userRepository = _unitOfWork.GetRepository<Account>();
        var roleRepository = _unitOfWork.GetRepository<Role>();

       
        var role = await roleRepository.SingleOrDefaultAsync(
            predicate: r => r.Id == createAccountRequest.RoleId
        );
        if (role == null)
        {
            throw new KeyNotFoundException($"Role with ID '{createAccountRequest.RoleId}' not found");
        }

         
        var emailExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Email == createAccountRequest.Email
        ) != null;
        if (emailExists)
        {
            throw new InvalidOperationException($"Email '{createAccountRequest.Email}' is already in use");
        }

     
        var usernameExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Username == createAccountRequest.Username
        ) != null;
        if (usernameExists)
        {
            throw new InvalidOperationException($"Username '{createAccountRequest.Username}' is already in use");
        }
         
        var user = createAccountRequest.Adapt<Account>();
         
        user.RoleId = role.Id;
        
        user.Status = "active";
       
        user.CreatedAt = DateTime.UtcNow;
        
        try
        {
            
            var createdUser = await userRepository.InsertAsync(user);
            await _unitOfWork.CommitAsync();

            
            return createdUser.Adapt<UserResponse>();
        }
        catch (Exception ex)
        {
             
            throw new InvalidOperationException("An error occurred while creating the user.", ex);
        }
    }




    public async Task<UserResponse> DeleteUserAsync(Guid id)
    {
        var userRepository = _unitOfWork.GetRepository<Account>();

         
        var user = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Id == id,
            include: query => query.Include(u => u.Role)  
        );

        if (user == null)
            throw new KeyNotFoundException("User not found");

        
        user.Status = user.Status == "active" ? "block" : "active";

        
        userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();

         
        var userResponse = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role?.Name ?? "N/A", 
            RoleId = user.RoleId,
            Status = user.Status,
            Avatar = user.Avatar,
            ConfirmationToken = user.ConfirmationToken,
            IsConfirmed = user.IsConfirmed
        };

        return userResponse;
    }



    public async Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(predicate: a => a.Id == id);
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

  
}