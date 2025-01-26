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
        [Route("api/admin/GetAllUseraccount")]
        [HttpGet]
        public async Task<IActionResult> GetAccUsersByAdmin([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            if (page <= 0 || size <= 0)
                return BadRequest("Page and size must be greater than 0.");

            
            var pagedUsers = await _accountService.GetPagedUsersAsync(page, size);

            return Ok(pagedUsers);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateCurrentAccount(Guid id, [FromForm] UpdateAccountRequest updateAccountRequest)
        {
            await _accountService.UpdateAccount(id, updateAccountRequest);
            return NoContent();
        }
    }
}
