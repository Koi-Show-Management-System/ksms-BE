using KSMS.Application.Services;
using KSMS.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Categorie;

namespace KSMS.API.Controllers
{
    [Route("api/v1/competition-category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateCategory([FromBody] CreateCompetitionCategoryRequest request)
        {
            await _categoryService.CreateCompetitionCategory(request);
            return StatusCode(201, ApiResponse<object>.Created(null, "Tạo hạng mục thành công"));
        }
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCategory(Guid id, [FromBody] UpdateCompetitionCategoryRequest request)
        {
            await _categoryService.UpdateCompetitionCategory(id, request);
            return Ok(ApiResponse<object>.Success(null, "Cập nhật hạng mục thành công"));
        }
        [HttpGet("get-page")]
        public async Task<ActionResult<ApiResponse<object>>> GetPageCategory(
            [FromQuery] Guid showId,
            [FromQuery] bool? hasTank = null,
            [FromQuery] int page = 1,     
            [FromQuery] int size = 10)    
        {
            var categories = await _categoryService.GetPagedCompetitionCategory(showId,hasTank, page, size);
            return Ok(ApiResponse<object>.Success(categories, "Lấy danh sách hạng mục thành công"));
        }
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<object>>> GetCategoryById(Guid id)
        {
            var category = await _categoryService.GetCompetitionCategoryDetailById(id);
            return Ok(ApiResponse<object>.Success(category, "Lấy chi tiết hạng mục thành công"));
        }
    }
}
