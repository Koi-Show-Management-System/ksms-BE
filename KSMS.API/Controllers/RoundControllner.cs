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
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace KSMS.API.Controllers
{
    [Route("api/v1/round")]
    [ApiController]
    public class RoundController : ControllerBase
    {
        private readonly IRoundService _roundService;
        public RoundController(IRoundService roundService)
        {
            _roundService = roundService;
        }
        [HttpPatch("update-status-roundid")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateRoundsStatusByKoiShow([FromQuery] Guid roundID)
        {
            await _roundService.UpdateRoundStatusAsync(roundID);
            return Ok(ApiResponse<object>.Success(null, "Round statuses updated successfully"));
        }
        [HttpGet("{competitionCategoryId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetPageRegistrationRounds(Guid competitionCategoryId, [FromQuery]RoundEnum roundType, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var registrationRounds = await _roundService.GetPageRound(competitionCategoryId, roundType, page, size);
            return Ok(ApiResponse<object>.Success(registrationRounds, "Get successfully"));
        }
        [HttpGet("get-round-type-for-referee/{competitionCategoryId:guid}")]
        [Authorize(Roles = "Referee")]
        public async Task<ActionResult<ApiResponse<object>>> GetRoundTypeForReferee(Guid competitionCategoryId)
        {
            var roundTypes = await _roundService.GetRoundTypeForReferee(competitionCategoryId);
            return Ok(ApiResponse<object>.Success(roundTypes, "Get successfully"));
        }
    }
}
