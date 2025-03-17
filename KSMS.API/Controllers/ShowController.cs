using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;
using KSMS.Domain.Dtos.Responses.Variety;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/v1/koi-show")]
    public class ShowController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowController(IShowService showService)
        {
            _showService = showService;
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateShow([FromBody] CreateShowRequest createShowRequest)
        {
             await _showService.CreateShowAsync(createShowRequest);
            return StatusCode(201, ApiResponse<object>.Created(null, "Create show successfully"));
        }
        
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetShowById(Guid id)
        {
            var show = await _showService.GetShowDetailByIdAsync(id);

            return Ok(ApiResponse<object>.Success(show, "Get show successfully"));
        }
        

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShow(Guid id, [FromBody] UpdateShowRequestV2 request)
        {
            await _showService.UpdateShowV2(id, request);
            return Ok(ApiResponse<object>.Success(null, "Update show successfully"));
        }

        [HttpGet("paged")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagedShows( [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var shows = await _showService.GetPagedShowsAsync(page, size);
            return Ok(ApiResponse<Paginate<PaginatedKoiShowResponse>>.Success(shows, "Get paged shows successfully"));
        }
        
        
    }
}
