using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Responses.Score;
using Microsoft.AspNetCore.Authorization;
using KSMS.Domain.Dtos.Requests.ScoreDetail;

namespace KSMS.API.Controllers
{
    [Route("api/v1/score")]
    [ApiController]
    public class ScoreController : ControllerBase
    {
        private readonly IScoreService _scoreService;

        public ScoreController(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }
        //[Route("get-all-referee-score")]
        //[HttpGet]
        //public async Task<ActionResult<ApiResponse<object>>> GetPagedScores([FromQuery] int page = 1, [FromQuery] int size = 10)
        //{
        //    var pagedScores = await _scoreService.GetPagedScoresAsync(page, size);
        //    return Ok(ApiResponse<object>.Success(pagedScores, "Get the list of score successfully"));
        //}

        [Route("create-score")]
        //   [Authorize(Roles = "Referee")] 
        [HttpPost]
        public async Task<ActionResult<ApiResponse<object>>> CreateScore([FromBody] CreateScoreDetailRequest request)
        {
            
             await _scoreService.CreateScoreAsync(request);
            return StatusCode(201, ApiResponse<object>.Created(null, "Create score successfully"));
        }
        /// <summary>
        /// Chấm điểm vòng loại (trọng tài chỉ chọn `Pass` hoặc `Fail`).
        /// </summary>
        [HttpPost("Preliminary")]
     //   [Authorize(Roles = "Referee")] 
        public async Task<ActionResult<ApiResponse<object>>> CreatenPreliminaryScore([FromBody] CreateEliminationScoreRequest request)
        {
           
            
                await _scoreService.CreateEliminationScoreAsync(request);
            return Ok(ApiResponse<ScoreDetailResponse>.Success(null, "Elimination score submitted successfully"));
            
        }
    }
}
