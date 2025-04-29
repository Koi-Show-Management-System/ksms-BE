using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Variety;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/variety")]
[ApiController]
public class VarietyController : ControllerBase
{
    private readonly IVarieryService _varietyService;

    public VarietyController(IVarieryService varietyService)
    {
        _varietyService = varietyService;
    }
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<object>>> CreateVariety([FromBody]CreateVarietyRequest createVarietyRequest)
    {
        await _varietyService.CreateVariety(createVarietyRequest);
        return StatusCode(201, ApiResponse<object>.Created(null, "Tạo giống cá thành công"));
    }
    [HttpGet("get-page")]
    public async Task<ActionResult<ApiResponse<object>>> GetPagingCriteria([FromQuery]int page = 1, [FromQuery]int size = 10)
    {
        var variety = await _varietyService.GetPagingVariety(page, size);
        return Ok(ApiResponse<object>.Success(variety, "Lấy danh sách giống cá thành công"));
    }

    [HttpPut("update/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateVariety(Guid id, [FromBody]UpdateVarietyRequest updateVarietyRequest)
    {
        await _varietyService.UpdateVariety(id, updateVarietyRequest);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật giống cá thành công"));
    }

    [HttpDelete("delete/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteVariety(Guid id)
    {
        await _varietyService.DeleteVariety(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa giống cá thành công"));
    }
}