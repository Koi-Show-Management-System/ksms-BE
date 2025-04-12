using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.BlogCategory;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/blog-category")]
[ApiController]
public class BlogCategoryController : ControllerBase
{
    private readonly IBlogCategoryService _blogCategoryService;

    public BlogCategoryController(IBlogCategoryService blogCategoryService)
    {
        _blogCategoryService = blogCategoryService;
    }
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<object>>> CreateBlogCategory([FromBody] CreateBlogCategoryRequest request)
    {
        await _blogCategoryService.CreateBlogCategory(request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Tạo danh mục blog thành công"));
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateBlogCategory(Guid id, [FromBody] UpdateBlogCategoryRequest request)
    {
        await _blogCategoryService.UpdateBlogCategory(id, request);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật danh mục blog thành công"));
    }
    [HttpGet("get-all")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllBlogCategory()
    {
        var categories = await _blogCategoryService.GetAll();
        return Ok(ApiResponse<object>.Success(categories, "Lấy danh sách danh mục blog thành công"));
    }
}