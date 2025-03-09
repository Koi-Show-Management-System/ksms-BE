using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Sponsor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/sponsor")]
[ApiController]
public class SponsorController : ControllerBase
{
    private readonly ISponsorService _sponsorService;

    public SponsorController(ISponsorService sponsorService)
    {
        _sponsorService = sponsorService;
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPost("create/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> CreateSponsor(Guid showId, [FromBody]CreateSponsorRequest request)
    {
        await _sponsorService.CreateSponsorAsync(showId, request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Create successfully"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateSponsor(Guid id,[FromBody] UpdateSponsorRequestV2 request)
    {
        await _sponsorService.UpdateSponsorAsync(id, request);
        return Ok(ApiResponse<object>.Success(null, "Update successfully"));
    }
    [HttpGet("get-page/{koiShowId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetPageSponsor(Guid koiShowId,
        [FromQuery] int page = 1, [FromQuery]int size  = 10)
    {
        var sponsors = await _sponsorService.GetPageSponsorAsync(koiShowId, page, size);
        return Ok(ApiResponse<object>.Success(sponsors, "Get list successfully"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteSponsor(Guid id)
    {
        await _sponsorService.DeleteSponsorAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Delete successfully"));
    }
}