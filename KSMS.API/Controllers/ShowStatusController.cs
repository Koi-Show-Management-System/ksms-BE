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
    public async Task<ActionResult<ApiResponse<object>>> CreateStatus(Guid showId, [FromBody]List<CreateShowStatusRequest> request)
    {
        await _showStatusService.CreateShowStatusAsync(showId, request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Tạo tiến trình triển lãm thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPut("{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateStatus(Guid showId,[FromBody] List<UpdateShowStatusRequestV2> request)
    {
        await _showStatusService.UpdateShowStatusAsync(showId, request);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật tiến trình triển lãm thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStatus(Guid id)
    {
        await _showStatusService.DeleteShowStatusAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa tiến trình triển lãm thành công"));
    }
}