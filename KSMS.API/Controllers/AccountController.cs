using KSMS.Application.GoogleServices;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using KSMS.Domain.Dtos;

namespace KSMS.API.Controllers
{
    [Route("api/v1/account")]
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
        public async Task<ActionResult<ApiResponse<object>>> ToggleUserStatus(Guid id, [FromQuery]AccountStatus status)
        {
            var updatedUser = await _accountService.UpdateStatus(id, status);
            var statusMessage = status switch
            {
                AccountStatus.Active => "kích hoạt",
                AccountStatus.Blocked => "khóa",
                AccountStatus.Deleted => "xóa",
                _ => status.ToString().ToLower()
            };
            return Ok(ApiResponse<object>.Success(updatedUser, $"Tài khoản đã được {statusMessage}"));
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateUser([FromForm] CreateAccountRequest createAccountRequest)
        {
            var newUser = await _accountService.CreateUserAsync(createAccountRequest);
            return StatusCode(201, ApiResponse<object>.Created(newUser, "Thêm tài khoản thành công"));
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserById(Guid id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            return Ok(ApiResponse<object>.Success(user, "Lấy thông tin tài khoản thành công"));
        }

        [Route("admin/get-paging-account")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAccUsersByAdmin([FromQuery] RoleName? roleName, [FromQuery] AccountStatus? status, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var pagedUsers = await _accountService.GetPagedUsersAsync(roleName, status, page, size);
            return Ok(ApiResponse<object>.Success(pagedUsers, "Lấy danh sách tài khoản thành công"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCurrentAccount(Guid id, [FromForm] UpdateAccountRequest updateAccountRequest)
        {
            await _accountService.UpdateAccount(id, updateAccountRequest);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật tài khoản thành công"));
        }
    }
}
