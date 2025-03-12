using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using KSMS.Infrastructure.Services;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [Route("api/v1/registration-round")]
    [ApiController]
    public class RegistrationRoundController : ControllerBase 
    {
        private readonly IRegistrationRoundService _registrationRoundService;
        public RegistrationRoundController(IRegistrationRoundService registrationRoundService)
        {
            _registrationRoundService = registrationRoundService;
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRegistrationRound([FromBody] CreateRegistrationRoundRequest request)
        {
            var newRegistrationRound = await _registrationRoundService.CreateRegistrationRoundAsync(request);
            return StatusCode(201, ApiResponse<object>.Created(newRegistrationRound, "Registration round created successfully"));
        }
        // quét mã qr cho trọng tài 
        [HttpGet("get-registration-round-for-referee")]
        public async Task<ActionResult<ApiResponse<CheckQrRegistrationRoundResponse>>> GetRegistrationRoundByIdAndRoundAsync(
            [FromQuery] Guid registrationId,
            [FromQuery] Guid roundId)
        {
            var registrationRoundInfo = await _registrationRoundService.GetRegistrationRoundByIdAndRoundAsync(registrationId, roundId);
            return Ok(ApiResponse<CheckQrRegistrationRoundResponse>.Success(registrationRoundInfo, "Fetched registration info successfully"));
        }
        [HttpGet("{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPageRegistrationRounds(Guid roundId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var registrationRounds = await _registrationRoundService.GetPageRegistrationRound(roundId, page, size);
            return Ok(ApiResponse<object>.Success(registrationRounds, "Get successfully"));
        }
    }
}
