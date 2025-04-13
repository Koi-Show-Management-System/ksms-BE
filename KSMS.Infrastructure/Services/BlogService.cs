using System.Linq.Expressions;
using KSMS.Application.Extensions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Blog;
using KSMS.Domain.Dtos.Responses.Blog;
using KSMS.Domain.Entities;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class BlogService : BaseService<BlogService>, IBlogService
{
    public BlogService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<BlogService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
    {
    }

    public async Task CreateBlog(CreateBlogRequest request)
    {
        var blogCategory = await _unitOfWork.GetRepository<BlogCategory>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.BlogCategoryId);
        if (blogCategory == null)
        {
            throw new Exception("Danh mục không tồn tại");
        }
        var existingBlog = await _unitOfWork.GetRepository<BlogsNews>().SingleOrDefaultAsync(
            predicate: p => p.Title.ToLower() == request.Title.ToLower());
        if (existingBlog != null)
        {
            throw new Exception("Tiêu đề tin tức đã tồn tại");
        }
        
        var blog = request.Adapt<BlogsNews>();
        blog.AccountId = GetIdFromJwt();
        await _unitOfWork.GetRepository<BlogsNews>().InsertAsync(blog);
        await _unitOfWork.CommitAsync();

    }

    public async Task UpdateBlog(Guid id, UpdateBlogRequest request)
    {
        var blog = await _unitOfWork.GetRepository<BlogsNews>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (blog == null)
        {
            throw new Exception("Tin tức không tồn tại");
        }
        var blogCategory = await _unitOfWork.GetRepository<BlogCategory>().SingleOrDefaultAsync(
            predicate: x => x.Id == request.BlogCategoryId);
        if (blogCategory == null)
        {
            throw new Exception("Danh mục không tồn tại");
        }
        var existingBlog = await _unitOfWork.GetRepository<BlogsNews>().SingleOrDefaultAsync(
            predicate: p => p.Title.ToLower() == request.Title.ToLower() && p.Id != id);
        if (existingBlog != null)
        {
            throw new Exception("Tiêu đề tin tức đã tồn tại");
        }
        request.Adapt(blog);
        blog.AccountId = GetIdFromJwt();
        _unitOfWork.GetRepository<BlogsNews>().UpdateAsync(blog);
        await _unitOfWork.CommitAsync();
       
    }

    public async Task<Paginate<GetPageBlogResponse>> GetAllBlogs(Guid? blogCategoryId, int page, int size)
    {
        Expression<Func<BlogsNews, bool>> filterQuery = blog => true;
        if (blogCategoryId != null)
        {
            filterQuery = filterQuery.AndAlso(b => b.BlogCategoryId == blogCategoryId);
        }

        var blogs = await _unitOfWork.GetRepository<BlogsNews>().GetPagingListAsync(
            predicate: filterQuery,
            include: query => query
                .Include(x => x.BlogCategory)
                .Include(x => x.Account),
            orderBy: query => query.OrderByDescending(x => x.CreatedAt),
            page: page,
            size: size);
        return blogs.Adapt<Paginate<GetPageBlogResponse>>();
    }

    public async Task<GetPageBlogResponse> GetBlogById(Guid id)
    {
        var blog = await _unitOfWork.GetRepository<BlogsNews>().SingleOrDefaultAsync(predicate: x => x.Id == id,
            include: query => query
                .Include(x => x.BlogCategory)
                .Include(x => x.Account));
        if (blog == null)
        {
            throw new Exception("Tin tức không tồn tại");
        }
        return blog.Adapt<GetPageBlogResponse>();
    }

    public async Task DeleteBlog(Guid id)
    {
        var blog = await _unitOfWork.GetRepository<BlogsNews>().SingleOrDefaultAsync(predicate: x => x.Id == id);
        if (blog == null)
        {
            throw new Exception("Tin tức không tồn tại");
        }
        _unitOfWork.GetRepository<BlogsNews>().DeleteAsync(blog);
        await _unitOfWork.CommitAsync();
    }
}