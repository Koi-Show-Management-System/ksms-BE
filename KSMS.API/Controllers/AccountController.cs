using KSMS.Application.GoogleServices;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using KSMS.Domain.Dtos;

namespace KSMS.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService, IFirebaseService firebaseService)
        {
            _accountService = accountService;
            _firebaseService = firebaseService;
        }
        [HttpPatch("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid id, AccountStatus status)
        {
            var updatedUser = await _accountService.UpdateStatus(id, status);
            var statusMessage = status.ToString().ToLower();
            return Ok(ApiResponse<object>.Success(updatedUser, $"Account has been {statusMessage}"));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateUser([FromBody] CreateAccountRequest createAccountRequest)
        {
            var newUser = await _accountService.CreateUserAsync(createAccountRequest);
            return StatusCode(201, ApiResponse<object>.Created(newUser, "Register account successfully"));
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserById(Guid id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            return Ok(ApiResponse<object>.Success(user, "Get account successfully"));
        }

        [Route("admin/get-paging-account")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAccUsersByAdmin([FromQuery] RoleName? roleName, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var pagedUsers = await _accountService.GetPagedUsersAsync(roleName, page, size);
            return Ok(ApiResponse<object>.Success(pagedUsers, "Get list of account successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCurrentAccount(Guid id, [FromForm] UpdateAccountRequest updateAccountRequest)
        {
            await _accountService.UpdateAccount(id, updateAccountRequest);
            return Ok(ApiResponse<object>.Success(null, "Update account successfully"));
        }
    }
}
