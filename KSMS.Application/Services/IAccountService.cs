using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IAccountService
{
    Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest);

    Task<Paginate<AccountResponse>> GetPagedUsersAsync(RoleName? roleName, int page, int pageSize);
    Task<AccountResponse> GetUserByIdAsync(Guid id);
    Task<AccountResponse> CreateUserAsync(CreateAccountRequest createAccountRequest);
    Task<AccountResponse> UpdateStatus(Guid id, AccountStatus status);
}