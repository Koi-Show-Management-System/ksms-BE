using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Dtos.Responses.Account;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IAccountService
{
    Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest);

    Task<Paginate<GetALLAccountResponse>> GetPagedUsersAsync(int page, int pageSize);
    Task<GetALLAccountResponse> GetUserByIdAsync(Guid id);
    Task<GetALLAccountResponse> CreateUserAsync(CreateAccountRequest createAccountRequest);
    Task<GetALLAccountResponse> UpdateStatus(Guid id, AccountStatus status);
}