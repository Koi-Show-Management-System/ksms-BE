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
                Status = account.Status ?? "active"
            },
            predicate: null,  
            orderBy: query => query.OrderBy(a => a.Username),  
            include: query => query.Include(a => a.Role),  
            page: page,  
            size: size   
        );

        return pagedUsers;
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