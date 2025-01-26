using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateCurrentAccount(Guid id, [FromForm] UpdateAccountRequest updateAccountRequest)
        {
            await _accountService.UpdateAccount(id, updateAccountRequest);
            return NoContent();
        }
    }
}
