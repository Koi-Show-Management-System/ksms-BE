using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace KSMS.API.Controllers
{
    [Route("api/v1/tank")]
    [ApiController]
    public class TankController : ControllerBase
    {
        private readonly ITankService _tankService;

        public TankController(ITankService tankService)
        {
            _tankService = tankService;
        }
        [HttpGet("{competitionCategoryId:guid}/paged")]
        public async Task<ActionResult<ApiResponse<Paginate<TankResponse>>>> GetPagedTanksByCompetitonCategoryId(
             Guid competitionCategoryId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var tanks = await _tankService.GetPagedTanksByCategoryIdAsync(competitionCategoryId, page, size);
            return Ok(ApiResponse<Paginate<TankResponse>>.Success(tanks, "Lấy danh sách bể thành công"));
        }

        /// <summary>
        /// API Update status tank
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateTankStatus(Guid id, [FromQuery] TankStatus status)
        {
            await _tankService.UpdateTankStatusAsync(id, status);
            return Ok(new { message = "Cập nhật trạng thái bể thành công" });
        }

        /// <summary>
        /// API tạo một Tank mới (không trả về response)
        /// </summary>
        [HttpPost("create")]
        [Authorize(Roles = "Admin, Staff, Manager")]
        public async Task<IActionResult> CreateTank([FromBody] CreateTankRequest tankRequest)
        {
            await _tankService.CreateTankAsync(tankRequest);
            return StatusCode(201, ApiResponse<object>.Success(null, "Tạo bể thành công"));
        }

     

        /// <summary>
        /// API cập nhật thông tin Tank theo ID (không trả về response)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin, Staff, Manager")]
        public async Task<IActionResult> UpdateTank(Guid id, [FromBody] UpdateTankRequest request)
        {
            await _tankService.UpdateTankAsync(id, request);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật bể thành công"));
        }
    }
}
