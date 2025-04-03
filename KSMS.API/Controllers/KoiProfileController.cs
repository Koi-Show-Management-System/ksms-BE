using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.KoiProfile;
using KSMS.Domain.Dtos.Responses.KoiProfile;
using KSMS.Domain.Models;
using KSMS.Domain.Pagination;
using KSMS.Domain.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers
{
    [Route("api/v1/koi-profile")]
    [ApiController]
    public class KoiProfileController : ControllerBase
    {
        private readonly IKoiProfileService _koiProfileService;

        public KoiProfileController(IKoiProfileService koiProfileService)
        {
            _koiProfileService = koiProfileService;
        }
        [HttpPost("create")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> CreateKoiProfile([FromForm] CreateKoiProfileRequest createKoiProfileRequest)
        { 
            var koi = await _koiProfileService.CreateKoiProfile(createKoiProfileRequest);
            return StatusCode(201, ApiResponse<object>.Created(koi, "Tạo hồ sơ Koi thành công"));
        }
        [HttpGet("get-page")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagedKoiProfile([FromQuery] KoiProfileFilter filter, [FromQuery]int page = 1, [FromQuery]int size = 10)
        {
            var result = await _koiProfileService.GetPagedKoiProfile(filter, page, size);
            return Ok(ApiResponse<object>.Success(result, "Lấy danh sách hồ sơ Koi thành công"));
            
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetKoiProfileById(Guid id)
        {
            var koi = await _koiProfileService.GetById(id);

            return Ok(ApiResponse<object>.Success(koi, "Lấy chi tiết hồ sơ Koi thành công"));
        }
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateKoiProfile(Guid id, [FromForm] UpdateKoiProfileRequest updateKoiProfileRequest)
        {
            await _koiProfileService.UpdateKoiProfile(id, updateKoiProfileRequest);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật hồ sơ Koi thành công"));
        }
    }
}
