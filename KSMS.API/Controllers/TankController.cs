using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [Route("api/tank")]
    [ApiController]
    public class TankController : ControllerBase
    {
        private readonly ITankService _tankService;

        public TankController(ITankService tankService)
        {
            _tankService = tankService;
        }
        [HttpGet("{koiShowId:guid}/paged")]
        public async Task<ActionResult<ApiResponse<Paginate<TankResponse>>>> GetPagedTanksByKoiShowId(
             Guid koiShowId, [FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var tanks = await _tankService.GetPagedTanksByKoiShowIdAsync(koiShowId, page, size);
            return Ok(ApiResponse<Paginate<TankResponse>>.Success(tanks, "Get paged tanks successfully"));
        }

        /// <summary>
        /// API tạo một Tank mới (không trả về response)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateTank([FromBody] CreateTankRequest tankRequest)
        {
            await _tankService.CreateTankAsync(tankRequest);
            return StatusCode(201, ApiResponse<object>.Success(null, "Create tank successfully"));
        }

     

        /// <summary>
        /// API cập nhật thông tin Tank theo ID (không trả về response)
        /// </summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateTank(Guid id, [FromBody] UpdateTankRequest request)
        {
            await _tankService.UpdateTankAsync(id, request);
            return Ok(ApiResponse<object>.Success(null, "Tank updated successfully"));
        }
    }
}
