using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Authentication;
using KSMS.Domain.Dtos.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequest request)
        {
            await _authenticationService.Register(request);
            return StatusCode(201, ApiResponse<object>.Created(null, "Đăng ký tài khoản thành công"));
        }
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginRequest request)
        {
            var response = await _authenticationService.Login(request);
            return StatusCode(201, ApiResponse<object>.Created(response, "Đăng nhập thành công"));
        }
        
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromQuery] string email)
        {
            await _authenticationService.SendForgotPasswordOTP(email);
            return Ok(ApiResponse<object>.Success(null, "Mã OTP đã được gửi đến email của bạn."));
        }

        [HttpPut("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _authenticationService.ResetPassword(request.Email, request.OTP, request.NewPassword);
            return Ok(ApiResponse<object>.Success(null, "Đặt lại mật khẩu thành công"));
        }
    }
}
