using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos;
using System;
using System.Threading.Tasks;
using KSMS.Infrastructure.Services;
using KSMS.Infrastructure.Utils;
using KSMS.Domain.Entities;
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Dtos.Responses.Registration;

namespace KSMS.API.Controllers
{
    [Route("api/v1/round")]
    [ApiController]
    public class RoundController : ControllerBase
    {
        private readonly IRoundService _roundService;
        private readonly IRegistrationService _registrationService;
        public RoundController(IRoundService roundService, IRegistrationService registrationService)
        {
            _roundService = roundService;
            
            _registrationService = registrationService;
        }
         

        

        [HttpPatch("update-status-roundid")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRoundsStatusByKoiShow([FromQuery] Guid roundID)
        {
            await _roundService.UpdateRoundStatusAsync(roundID);
            return Ok(ApiResponse<object>.Success(null, "Round statuses updated successfully"));
        }
    }
}
