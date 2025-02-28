using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace KSMS.API.Controllers
{
    [Route("api/criteria")]
    [ApiController]
    public class CriteriaController : ControllerBase
    {
        private readonly ICriterionService _criterionService;

        public CriteriaController(ICriterionService criterionService)
        {
            _criterionService = criterionService;
        }


        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateCriterion([FromBody] CreateCriteriaRequest createCriteriaRequest)
        {
            await _criterionService.CreateCriteriaAsync(createCriteriaRequest);
            return StatusCode(201, ApiResponse<object>.Created(null, "Criterion created successfully"));
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCriterionById(Guid id)
        {
            var criterion = await _criterionService.GetCriteriaByIdAsync(id);
            return Ok(ApiResponse<object>.Success(criterion, "Criterion retrieved successfully"));
        }


        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCriterion(Guid id, [FromBody] UpdateCriteriaRequest updateCriteriaRequest)
        { 
            await _criterionService.UpdateCriteriaAsync(id, updateCriteriaRequest);
            return Ok(ApiResponse<object>.Success(null, "Criterion updated successfully"));
        }

        [HttpGet("get-page")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagingCriteria([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var criteria = await _criterionService.GetPagingCriteria(page, size);
            return Ok(ApiResponse<object>.Success(criteria, "Get list  successfully"));
        }

        //[HttpDelete("{id:guid}")]
        //public async Task<ActionResult<ApiResponse<object>>> DeleteCriterion(Guid id)
        //{
        //    await _criterionService.DeleteCriterionAsync(id);
        //    return Ok(ApiResponse<object>.Success(null, "Criterion deleted successfully"));
        //}


    }
}