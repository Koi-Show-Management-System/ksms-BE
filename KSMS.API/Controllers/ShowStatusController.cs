using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Dtos.Requests.ShowStatus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/show-status")]
[ApiController]
public class ShowStatusController : ControllerBase
{
    private readonly IShowStatusService _showStatusService;

    public ShowStatusController(IShowStatusService showStatusService)
    {
        _showStatusService = showStatusService;
    }
    
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPost("create/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> CreateStatus(Guid showId, [FromBody]CreateShowStatusRequest request)
    {
        await _showStatusService.CreateShowStatusAsync(showId, request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Create successfully"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid id,[FromBody] UpdateShowStatusRequestV2 request)
    {
        await _showStatusService.UpdateShowStatusAsync(id, request);
        return Ok(ApiResponse<object>.Success(null, "Update successfully"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStatus(Guid id)
    {
        await _showStatusService.DeleteShowStatusAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Delete successfully"));
    }
}