using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetPagedScores([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var pagedScores = await _scoreService.GetPagedScoresAsync(page, size);
            return Ok(pagedScores);
        }
         
        [Route("Refree/CreateScores")]
        [HttpPost]
        public async Task<IActionResult> CreateScore([FromBody] CreateScoreRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _scoreService.CreateScoreAsync(request);
            return Ok(response);
        }

    }
}
