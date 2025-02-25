using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Tank;
using KSMS.Domain.Dtos.Responses.Tank;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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

        
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateTank([FromBody] TankRequest tankRequest)
        {
            var newTank = await _tankService.CreateTankAsync(tankRequest);
            return StatusCode(201, ApiResponse<object>.Created(newTank, "Create tank successfully"));
        }

        
        [HttpGet("{koiShowId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetTanksByKoiShowId(Guid koiShowId)
        {
            var tanks = await _tankService.GetTanksByKoiShowIdAsync(koiShowId);
            return Ok(ApiResponse<object>.Success(tanks, "Get tanks successfully"));
        }

         
        //[HttpPatch("{id:guid}")]
        //public async Task<ActionResult<ApiResponse<object>>> UpdateTankStatus(Guid id, [FromQuery] string status)
        //{
        //    var updatedTank = await _tankService.UpdateTankStatusAsync(id, status);
        //    return Ok(ApiResponse<object>.Success(updatedTank, "Tank status updated successfully"));
        //}
    }
}
