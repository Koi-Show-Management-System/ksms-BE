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
}