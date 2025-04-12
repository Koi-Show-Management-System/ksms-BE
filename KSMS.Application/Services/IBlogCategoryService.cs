using KSMS.Domain.Dtos.Requests.BlogCategory;

namespace KSMS.Application.Services;

public interface IBlogCategoryService
{
    Task CreateBlogCategory(CreateBlogCategoryRequest request);
    Task UpdateBlogCategory(Guid id, CreateBlogCategoryRequest request);
    //Task
}