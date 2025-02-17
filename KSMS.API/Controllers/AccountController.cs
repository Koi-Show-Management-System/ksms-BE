using KSMS.Application.GoogleServices;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Account;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> ToggleUserStatus(Guid id, AccountStatus status)
        {
            var updatedUser = await _accountService.UpdateStatus(id, status);
            var statusMessage = status.ToString().ToLower();
            return Ok(new { Message = $"User has been {statusMessage}.", User = updatedUser });

        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateAccountRequest createAccountRequest)
        {
            var newUser = await _accountService.CreateUserAsync(createAccountRequest);
            return CreatedAtAction(nameof(GetAccUsersByAdmin), new { id = newUser.Id }, newUser);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [Route("admin/GetAllUseraccount")]
        [HttpGet]
        public async Task<IActionResult> GetAccUsersByAdmin([FromQuery] int page = 1, [FromQuery] int size = 10)
        {

            
            var pagedUsers = await _accountService.GetPagedUsersAsync(page, size);

            return Ok(pagedUsers);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult> UpdateCurrentAccount(Guid id, [FromForm] UpdateAccountRequest updateAccountRequest)
        {
            await _accountService.UpdateAccount(id, updateAccountRequest);
            return NoContent();
        }
        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> UploadVideos(List<IFormFile> videoFiles)
        {
            try
            {
                string folderPath = "videos"; // Thư mục lưu trữ trên Firebase Storage
                var videoUrls = await _firebaseService.UploadVideosAsync(videoFiles, folderPath);

                return Ok(new { Message = "Upload thành công", VideoUrls = videoUrls });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
