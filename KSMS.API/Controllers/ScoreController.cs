using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using KSMS.Domain.Dtos;

namespace KSMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }
        [Route("admin/GetAllScores")]
        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetPagedScores([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var pagedScores = await _scoreService.GetPagedScoresAsync(page, size);
            return Ok(ApiResponse<object>.Success(pagedScores, "Get the list of score successfully"));
        }

        [Route("Refree/CreateScores")]
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateScore([FromBody] CreateScoreDetailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ModelState.ToString()));

            var response = await _scoreService.CreateScoreAsync(request);
            return StatusCode(201, ApiResponse<object>.Created(response, "Create score successfully"));
        }

    }
}
