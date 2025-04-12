using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.BlogCategory;
using KSMS.Domain.Dtos.Responses.BlogCategory;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class BlogCategoryService : BaseService<BlogCategoryService>, IBlogCategoryService
{
    public BlogCategoryService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<BlogCategoryService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateBlogCategory(CreateBlogCategoryRequest request)
    {
        var existBlogCategory = await _unitOfWork.GetRepository<BlogCategory>().SingleOrDefaultAsync(
            predicate: x => x.Name.ToLower() == request.Name.ToLower());
        if (existBlogCategory != null)
        {
            throw new BadRequestException("Tên danh mục đã tồn tại");
        }
        await _unitOfWork.GetRepository<BlogCategory>().InsertAsync(request.Adapt<BlogCategory>());
        await _unitOfWork.CommitAsync();
    }

    public async Task UpdateBlogCategory(Guid id, UpdateBlogCategoryRequest request)
    {
        var blogCategory = await _unitOfWork.GetRepository<BlogCategory>().SingleOrDefaultAsync(
            predicate: x => x.Id == id);
        if (blogCategory == null)
        {
            throw new NotFoundException("Không tìm thấy danh mục");
        }
        var existBlogCategory = await _unitOfWork.GetRepository<BlogCategory>().SingleOrDefaultAsync(
            predicate: x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id);
        if (existBlogCategory != null)
        {
            throw new BadRequestException("Tên danh mục đã tồn tại");
        }
        request.Adapt(blogCategory);
        _unitOfWork.GetRepository<BlogCategory>().UpdateAsync(blogCategory);
        await _unitOfWork.CommitAsync();

    }

    public async Task<List<GetBlogCategoryResponse>> GetAll()
    {
        var blogCategory = await _unitOfWork.GetRepository<BlogCategory>().GetListAsync();
        return blogCategory.Adapt<List<GetBlogCategoryResponse>>();
    }
}