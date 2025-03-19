using Microsoft.AspNetCore.Mvc;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos;
using System.Threading.Tasks;
using KSMS.Domain.Enums;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Services;

namespace KSMS.API.Controllers
{
    [Route("api/v1/round-result")]
    [ApiController]
    public class RoundResultController : ControllerBase
    {
        private readonly IRoundResultService _roundResultService;

        public RoundResultController(IRoundResultService roundResultService)
        {
            _roundResultService = roundResultService;
        }

        //[HttpPost("create")]
        //public async Task<ActionResult<ApiResponse<object>>> CreateRoundResult([FromBody] CreateRoundResult request)
        //{

        //    var createdRoundResult = await _roundResultService.CreateRoundResultAsync(request); 

        //    return StatusCode(201, ApiResponse<object>.Created(createdRoundResult, "Round result created successfully"));
        //}

        [HttpPost("finalize-round/{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> FinalizeRoundScores(Guid roundId)
        {
            try
            {
                await _roundResultService.ProcessFinalScoresForRound(roundId); // 🔥 THÊM `await` ĐỂ BẮT LỖI

                return StatusCode(201, ApiResponse<object>.Created(null, "Final scores calculated successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Fail($"Failed to process final scores: {ex.Message}"));
            }
        }
        
        [HttpPut("publish-round-result/{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> PublishRoundResult(Guid roundId)
        {
            await _roundResultService.PublishRoundResult(roundId);
            return Ok(ApiResponse<object>.Success(null, "Published round result successfully"));
        }

        // phân trang danh sách đăng kí theo category và status pass hay k
        [HttpGet("paged-RoundResult-registrations-by-category")]
        public async Task<ActionResult<ApiResponse<Paginate<RegistrationGetByCategoryPagedResponse>>>> GetPagedRegistrationsByCategoryAndStatusAsync([FromQuery] Guid categoryId, [FromQuery] RoundResultStatus? status, [FromQuery] int page =1, [FromQuery] int size = 10)
        {
            var pagedRegistrations = await _roundResultService.GetPagedRegistrationsByCategoryAndStatusAsync(categoryId, status, page, size);
            return Ok(ApiResponse<Paginate<RegistrationGetByCategoryPagedResponse>>.Success(pagedRegistrations, "Fetched paged registrations successfully"));
        }


        // public điểm của từng cá theo category
        [HttpPatch("update-isPublic-status-roundresultByCategoryid")]
        public async Task<ActionResult<ApiResponse<object>>> UpdatePublicStatus(Guid categoryId, [FromQuery] bool isPublic)
        {

              await _roundResultService.UpdateIsPublicByCategoryIdAsync(categoryId, isPublic);


            return Ok(ApiResponse<object>.Success(null, "Updated IsPublic status successfully"));
        }
    }
}
