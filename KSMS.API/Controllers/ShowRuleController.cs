using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/show-rule")]
[ApiController]
public class ShowRuleController : ControllerBase
{
    private readonly IShowRuleService _showRuleService;

    public ShowRuleController(IShowRuleService showRuleService)
    {
        _showRuleService = showRuleService;
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPost("create/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> CreateRule(Guid showId, [FromBody]CreateShowRuleRequest request)
    {
        await _showRuleService.CreateShowRuleAsync(showId, request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Tạo quy tắc thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRule(Guid id,[FromBody] UpdateShowRuleRequestV2 request)
    {
        await _showRuleService.UpdateShowRuleAsync(id, request);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật quy tắc thành công"));
    }
    [HttpGet("get-page/{showId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllRules(Guid showId,
        [FromQuery] int page = 1, [FromQuery]int size  = 10)
    {
        var rules = await _showRuleService.GetPageShowRuleAsync(showId, page, size);
        return Ok(ApiResponse<object>.Success(rules, "Lấy danh sách quy tắc thành công"));
    }
    [Authorize(Roles = "Admin, Manager, Staff")]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRule(Guid id)
    {
        await _showRuleService.DeleteShowRuleAsync(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa quy tắc thành công"));
    }
    
}