using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.RegistrationRound;
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
        //[HttpPost("generate-qr-registration")]
        //public ActionResult<ApiResponse<string>> GenerateQrCode([FromBody] Guid registrationID)
        //{
        //    var qrCodeBase64 = QrcodeUtil.GenerateQrCode(registrationID);
        //    return Ok(ApiResponse<string>.Success(qrCodeBase64, "Tạo mã QR thành công"));
        //}

        [HttpGet("{registrationId:guid}/{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetRegistrationRound(Guid registrationId, Guid roundId)
        {
            var registrationRound = await _registrationRoundService.GetRegistrationRoundAsync(registrationId, roundId);
            return Ok(ApiResponse<object>.Success(registrationRound, "Registration round details retrieved successfully"));
        }
    }
}
