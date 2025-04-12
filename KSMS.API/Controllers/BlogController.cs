using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.Blog;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/blog")]
[ApiController]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }
    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<object>>> CreateBlog([FromBody] CreateBlogRequest request)
    {
        await _blogService.CreateBlog(request);
        return StatusCode(201, ApiResponse<object>.Created(null, "Thêm tin tức thành công"));
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateBlog(Guid id, [FromBody] UpdateBlogRequest request)
    {
        await _blogService.UpdateBlog(id, request);
        return Ok(ApiResponse<object>.Success(null, "Cập nhật tin tức thành công"));
    }
    [HttpGet("get-page")]
    public async Task<ActionResult<ApiResponse<object>>> GetAllBlogs([FromQuery] Guid? blogCategoryId, [FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var pagedBlogs = await _blogService.GetAllBlogs(blogCategoryId, page, size);
        return Ok(ApiResponse<object>.Success(pagedBlogs, "Lấy danh sách tin tức thành công"));
    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetBlogById(Guid id)
    {
        var blog = await _blogService.GetBlogById(id);
        return Ok(ApiResponse<object>.Success(blog, "Lấy thông tin tin tức thành công"));
    }
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBlog(Guid id)
    {
        await _blogService.DeleteBlog(id);
        return Ok(ApiResponse<object>.Success(null, "Xóa tin tức thành công"));
    }
}