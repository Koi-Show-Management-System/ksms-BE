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
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> ToggleUserStatus(Guid id)
        {
            try
            {
                var updatedUser = await _accountService.DeleteUserAsync(id);  
                var statusMessage = updatedUser.Status == "block" ? "blocked" : "activated";
                return Ok(new { Message = $"User has been {statusMessage}.", User = updatedUser });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateAccountRequest createAccountRequest)
        {
            try
            {
                var newUser = await _accountService.CreateUserAsync(createAccountRequest);
                return CreatedAtAction(nameof(GetAccUsersByAdmin), new { id = newUser.Id }, newUser);
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        [Route("admin/GetAllUseraccount")]
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
