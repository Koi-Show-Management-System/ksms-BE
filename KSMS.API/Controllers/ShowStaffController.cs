using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/show-staff-manager")]
[ApiController]
public class ShowStaffController : ControllerBase
{
    
    private readonly IShowStaffService _showStaffService;

    public ShowStaffController(IShowStaffService showStaffService)
    {
        _showStaffService = showStaffService;
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpGet("get-page/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetPageStaffAndManager(
        Guid showId,
        [FromQuery] RoleName? role,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        var response = await _showStaffService.GetPageStaffAndManager(showId, role, page, size);
        return Ok(ApiResponse<object>.Success(response, "Lấy danh sách nhân viên và quản lý thành công"));
    }
    [Authorize(Roles = "Admin, Manager")]
    [HttpPost("add-staff-or-manager/{showId:guid}/{accountId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> AddStaffOrManager(Guid showId, Guid accountId)
    {
        await _showStaffService.AddStaffOrManager(showId, accountId);
        return StatusCode(201, ApiResponse<object>.Created(null, "Thêm nhân viên hoặc quản lý thành công"));
    }
    [Authorize(Roles = "Admin, Manager")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveStaffOrManager(Guid id)
    {
        await _showStaffService.RemoveStaffOrManager(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa nhân viên hoặc quản lý thành công"));
    }
}