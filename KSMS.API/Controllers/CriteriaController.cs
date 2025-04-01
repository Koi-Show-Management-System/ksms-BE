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
    [Route("api/v1/criteria")]
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
            return StatusCode(201, ApiResponse<object>.Created(null, "Tạo tiêu chí thành công"));
        }


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCriterionById(Guid id)
        {
            var criterion = await _criterionService.GetCriteriaByIdAsync(id);
            return Ok(ApiResponse<object>.Success(criterion, "Lấy chi tiết tiêu chí thành công"));
        }


        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCriterion(Guid id, [FromBody] UpdateCriteriaRequest updateCriteriaRequest)
        { 
            await _criterionService.UpdateCriteriaAsync(id, updateCriteriaRequest);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật tiêu chí thành công"));
        }

        [HttpGet("get-page")]
        public async Task<ActionResult<ApiResponse<object>>> GetPagingCriteria([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            var criteria = await _criterionService.GetPagingCriteria(page, size);
            return Ok(ApiResponse<object>.Success(criteria, "Lấy danh sách tiêu chí thành công"));
        }
        [HttpGet("get-list-criteria-competition-category/{competitionCategoryId:guid}/{roundId:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCriteriaCompetitionCategory(Guid competitionCategoryId, Guid roundId)
        {
            var criteria = await _criterionService.GetCriteriaCompetitionCategory(competitionCategoryId, roundId);
            return Ok(ApiResponse<object>.Success(criteria, "Lấy danh sách tiêu chí hạng mục thành công"));
        }
        //[HttpDelete("{id:guid}")]
        //public async Task<ActionResult<ApiResponse<object>>> DeleteCriterion(Guid id)
        //{
        //    await _criterionService.DeleteCriterionAsync(id);
        //    return Ok(ApiResponse<object>.Success(null, "Criterion deleted successfully"));
        //}


    }
}