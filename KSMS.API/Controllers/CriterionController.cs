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
    [Route("api/criterion")]
    [ApiController]
    public class CriterionController : ControllerBase
    {
        private readonly ICriterionService _criterionService;

        public CriterionController(ICriterionService criterionService)
        {
            _criterionService = criterionService;
        }

     
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateCriterion([FromBody] CreateCriterionRequest createCriterionRequest)
        {
            var createdCriterion = await _criterionService.CreateCriterionAsync(createCriterionRequest);
            return StatusCode(201, ApiResponse<object>.Created(createdCriterion, "Criterion created successfully"));
        }

   
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCriterionById(Guid id)
        {
            var criterion = await _criterionService.GetCriterionByIdAsync(id);
            return Ok(ApiResponse<object>.Success(criterion, "Criterion retrieved successfully"));
        }

   
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCriterion(Guid id, [FromBody] UpdateCriterionRequest updateCriterionRequest)
        {
            var updatedCriterion = await _criterionService.UpdateCriterionAsync(id, updateCriterionRequest);
            return Ok(ApiResponse<object>.Success(updatedCriterion, "Criterion updated successfully"));
        }

   
        //[HttpDelete("{id:guid}")]
        //public async Task<ActionResult<ApiResponse<object>>> DeleteCriterion(Guid id)
        //{
        //    await _criterionService.DeleteCriterionAsync(id);
        //    return Ok(ApiResponse<object>.Success(null, "Criterion deleted successfully"));
        //}

        
    }
}
