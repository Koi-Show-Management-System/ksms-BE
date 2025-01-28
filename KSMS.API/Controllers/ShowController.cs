using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> CreateShow([FromBody] CreateShowRequest createShowRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var showResponse = await _showService.CreateShowAsync(createShowRequest);
                return CreatedAtAction(nameof(GetShowById), new { id = showResponse.Id }, showResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get a show by its ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetShowById(Guid id)
        {
            try
            {
                var showResponse = await _showService.GetShowByIdAsync(id);
                if (showResponse == null)
                {
                    return NotFound(new { Error = "Show not found." });
                }

                return Ok(showResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
        }

        /// <summary>
        /// Get a paginated list of shows.
        /// </summary>
        //[HttpGet("list")]
        //public async Task<IActionResult> GetShows([FromQuery] int page = 1, [FromQuery] int size = 10)
        //{
        //    try
        //    {
        //        var shows = await _showService.GetPagedShowsAsync(page, size);
        //        return Ok(shows);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
        //    }
        //}

        /// <summary>
        /// Update the status of a show.
        /// </summary>
     //   [HttpPatch("{id:guid}/status")]
        //public async Task<IActionResult> UpdateShowStatus(Guid id, [FromBody] UpdateShowStatusRequest updateRequest)
        //{
        //    try
        //    {
        //        var updatedShow = await _showService.UpdateShowStatusAsync(id, updateRequest);
        //        return Ok(updatedShow);
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
