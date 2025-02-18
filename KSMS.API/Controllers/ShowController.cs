using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using KSMS.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/koi-show")]
    public class ShowController : ControllerBase
    {
        private readonly IShowService _showService;

        public ShowController(IShowService showService)
        {
            _showService = showService;
        }

        /// <summary>
        /// Create a new show with related entities.
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateShow([FromBody] CreateShowRequest createShowRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ModelState.ToString()));

            var showResponse = await _showService.CreateShowAsync(createShowRequest);
            return StatusCode(201, ApiResponse<object>.Created(showResponse, "Create show successfully"));
        }

        /// <summary>
        /// Get a show by its ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetShowById(Guid id)
        {
            var showResponse = await _showService.GetShowByIdAsync(id);
            if (showResponse == null)
                return NotFound(ApiResponse<object>.Fail("Show is not existed"));

            return Ok(ApiResponse<KoiShowResponse>.Success(showResponse, "Get show successfully"));
        }

        // <summary>
        // Get a paginated list of shows.
        // </summary>
        [HttpGet("list-show")]
        public async Task<ActionResult<ApiResponse<object>>> GetShows()
        {
            var shows = await _showService.GetAllShowsAsync();
            return Ok(ApiResponse<object>.Success(shows, "Get list of show successfully"));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateShow(Guid id, [FromBody] UpdateShowRequest updateRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail(ModelState.ToString()));

            await _showService.UpdateShowAsync(id, updateRequest);
            return Ok(ApiResponse<object>.Success(null, "Update show status successfully"));
        }



        /// <summary>
        /// Delete a show (soft delete).
        /// </summary>
        //[HttpDelete("{id:guid}")]
        //public async Task<IActionResult> DeleteShow(Guid id)
        //{
        //    try
        //    {
        //        await _showService.DeleteShowAsync(id);
        //        return Ok(new { Message = "Show deleted successfully." });
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(new { Error = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
        //    }
        //}
    }
}
