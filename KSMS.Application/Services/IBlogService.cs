using KSMS.Domain.Dtos.Requests.Blog;
using KSMS.Domain.Dtos.Responses.Blog;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface IBlogService
{
    Task CreateBlog(CreateBlogRequest request);
    Task UpdateBlog(Guid id, UpdateBlogRequest request);
    Task<Paginate<GetPageBlogResponse>> GetAllBlogs(Guid? blogCategoryId, int page, int size);
    Task<GetPageBlogResponse> GetBlogById(Guid id);
    Task DeleteBlog(Guid id);
}