using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [Route("api/round-result")]
    [ApiController]
    public class RoundResultController : ControllerBase
    {
        private readonly IRoundResultService _roundResultService;

        public RoundResultController(IRoundResultService roundResultService)
        {
            _roundResultService = roundResultService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateRoundResult([FromBody] CreateRoundResult request)
        {

            var createdRoundResult = await _roundResultService.CreateRoundResultAsync(request);


            return StatusCode(201, ApiResponse<object>.Created(createdRoundResult, "Round result created successfully"));
        }


        [HttpPatch("{id:guid}/update-public-status")]
        public async Task<ActionResult<ApiResponse<object>>> UpdatePublicStatus(Guid id, [FromBody] bool isPublic)
        {

            var updatedRoundResult = await _roundResultService.UpdateIsPublicAsync(id, isPublic);


            return Ok(ApiResponse<object>.Success(updatedRoundResult, "Updated IsPublic status successfully"));
        }
    }
}
