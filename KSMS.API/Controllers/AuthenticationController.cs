using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Authentication;
using KSMS.Domain.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            await _authenticationService.Register(registerRequest);
            return Ok();
        }
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            return Created(nameof(Login), await _authenticationService.Login(loginRequest));
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromQuery] string email)
        {
            await _authenticationService.SendForgotPasswordOTP(email);
            return Ok(new { Message = "Mã OTP đã được gửi đến email của bạn." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _authenticationService.ResetPassword(request.Email, request.OTP, request.NewPassword);
            return Ok(new { Message = "Đặt lại mật khẩu thành công." });
        }
    }
}
