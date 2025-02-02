using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.Show;
using KSMS.Domain.Exceptions;
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

        // <summary>
        // Get a paginated list of shows.
        // </summary>
        [HttpGet("listShow")]
        public async Task<IActionResult> GetShows()
        {
           
                var shows = await _showService.GetAllShowsAsync();
                return Ok(shows);
           
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateShow(Guid id, [FromBody] UpdateShowRequest updateRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                await _showService.UpdateShowAsync(id, updateRequest);

                return Ok(new { Message = "Show updated successfully." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred.", Details = ex.Message });
            }
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
