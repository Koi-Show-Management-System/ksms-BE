using KSMS.Domain.Dtos.Requests.BlogCategory;
using KSMS.Domain.Dtos.Responses.BlogCategory;

namespace KSMS.Application.Services;

public interface IBlogCategoryService
{
    Task CreateBlogCategory(CreateBlogCategoryRequest request);
    Task UpdateBlogCategory(Guid id, UpdateBlogCategoryRequest request);
    Task<List<GetBlogCategoryResponse>> GetAll();
}