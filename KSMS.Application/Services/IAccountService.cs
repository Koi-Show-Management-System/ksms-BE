using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IAccountService
{
    Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest);

    Task<IPaginate<UserResponse>> GetPagedUsersAsync(int page, int pageSize);
    Task<UserResponse> GetUserByIdAsync(Guid id);
    Task<UserResponse> CreateUserAsync(CreateAccountRequest createAccountRequest);
    Task<UserResponse> DeleteUserAsync(Guid id);
}