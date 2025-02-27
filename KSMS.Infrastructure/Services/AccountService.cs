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
using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;

namespace KSMS.Infrastructure.Services;

public class AccountService : BaseService<AccountService>, IAccountService
{
    private readonly IFirebaseService _firebaseService;
    public AccountService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<AccountService> logger, IHttpContextAccessor httpContextAccessor, IFirebaseService firebaseService) : base(unitOfWork, logger, httpContextAccessor)
    {
        _firebaseService = firebaseService;
    }
    public async Task<Paginate<GetALLAccountResponse>> GetPagedUsersAsync(int page, int size)
    { 
        var userRepository = _unitOfWork.GetRepository<Account>();

        
       var pagedUsers = await userRepository.GetPagingListAsync(
            orderBy: query => query.OrderBy(a => a.Username),  
            page: page,  
            size: size   
        );
        return pagedUsers.Adapt<Paginate<GetALLAccountResponse>>();
    }
    public async Task<GetALLAccountResponse> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.GetRepository<Account>()
            .SingleOrDefaultAsync(
                predicate: u => u.Id == id
            );

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        return user.Adapt<GetALLAccountResponse>();
        
    }

    public async Task<GetALLAccountResponse> CreateUserAsync(CreateAccountRequest createAccountRequest)
    {
         
        var userRepository = _unitOfWork.GetRepository<Account>();
        
        var emailExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Email == createAccountRequest.Email
        );
        if (emailExists!= null)
        {
            throw new BadRequestException($"Email '{createAccountRequest.Email}' is already in use");
        }
        var usernameExists = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Username == createAccountRequest.Username
        );
        if (usernameExists != null)
        {
            throw new BadRequestException($"Username '{createAccountRequest.Username}' is already in use");
        }
         
        var user = createAccountRequest.Adapt<Account>();
        user.HashedPassword = PasswordUtil.HashPassword(createAccountRequest.HashedPassword);
        user.IsConfirmed = true;
        var createdUser = await userRepository.InsertAsync(user);
        await _unitOfWork.CommitAsync();
        return createdUser.Adapt<GetALLAccountResponse>();
    }




    public async Task<GetALLAccountResponse> UpdateStatus(Guid id, AccountStatus status)
    {
        var userRepository = _unitOfWork.GetRepository<Account>();

         
        var user = await userRepository.SingleOrDefaultAsync(
            predicate: u => u.Id == id
        );

        if (user == null)
            throw new NotFoundException("User not found");
        
        
        user.Status = status switch
        {
            AccountStatus.Blocked => AccountStatus.Blocked.ToString().ToLower(),
            AccountStatus.Deleted => AccountStatus.Deleted.ToString().ToLower(),
            AccountStatus.Active => AccountStatus.Active.ToString().ToLower(),
            _ => user.Status
        };
        userRepository.UpdateAsync(user);
        await _unitOfWork.CommitAsync();
        return user.Adapt<GetALLAccountResponse>();
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