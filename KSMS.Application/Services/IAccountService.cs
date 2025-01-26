using KSMS.Domain.Dtos.Requests.Account;

namespace KSMS.Application.Services;

public interface IAccountService
{
    Task UpdateAccount(Guid id, UpdateAccountRequest updateAccountRequest);
}