using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Registration;
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

        // thả hết cá vào hồ theo roundi và đơn đăng kí 
        [HttpPost("assign-to-tank")]
        public async Task<ActionResult<ApiResponse<object>>> AssignMultipleFishesToTankAndRound(
    [FromBody] AssignFishesRequest request, [FromQuery] Guid? currentRoundId)
        {
            try
            {
                await _registrationRoundService.AssignMultipleFishesToTankAndRound(currentRoundId, request.RoundId, request.RegistrationIds);
                return Ok(ApiResponse<object>.Success(null, "Gán cá vào vòng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPost("assign-to-first-round")]
        public async Task<ActionResult<ApiResponse<object>>> AssignTFirstRound(
            [FromBody] AssignToFirstRound request)
        {
            try
            {
                await _registrationRoundService.AssignRegistrationsToPreliminaryRound(request.CategoryId, request.RegistrationIds);
                return Ok(ApiResponse<object>.Success(null, "Gán cá vào vòng thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
        [HttpPut("update-fish-tank")]
        //   [Authorize(Roles = "Staff, Admin, Manager")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateFishesTank(
     [FromBody] List<UpdateFishTankRequest> updateRequests)
        {
            try
            {
                await _registrationRoundService.UpdateFishesWithTanks(updateRequests);
                return Ok(ApiResponse<object>.Success(null, "Gán cá vào bể thành công"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }



        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRegistrationRound([FromBody] CreateRegistrationRoundRequest request)
        {
            var newRegistrationRound = await _registrationRoundService.CreateRegistrationRoundAsync(request);
            return StatusCode(201, ApiResponse<object>.Created(newRegistrationRound, "Registration round created successfully"));
        }
        [HttpPut("publish-registration-round/{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> PublishRegistrationRound(Guid roundId)
        {
            await _registrationRoundService.PublishRound(roundId);
            return Ok(ApiResponse<object>.Success(null, "Công bố vòng thi thành công"));
        }
        // quét mã qr cho trọng tài 
        [HttpGet("get-registration-round-for-referee")]
        public async Task<ActionResult<ApiResponse<CheckQrRegistrationRoundResponse>>> GetRegistrationRoundByIdAndRoundAsync(
            [FromQuery] Guid registrationId,
            [FromQuery] Guid roundId)
        {
            var registrationRoundInfo = await _registrationRoundService.GetRegistrationRoundByIdAndRoundAsync(registrationId, roundId);
            return Ok(ApiResponse<CheckQrRegistrationRoundResponse>.Success(registrationRoundInfo, "QUét mã QR thành công"));
        }
        [HttpGet("{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPageRegistrationRounds(Guid roundId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var registrationRounds = await _registrationRoundService.GetPageRegistrationRound(roundId, page, size);
            return Ok(ApiResponse<object>.Success(registrationRounds, "Lấy danh sách vòng thi thành công"));
        }
    }
}
