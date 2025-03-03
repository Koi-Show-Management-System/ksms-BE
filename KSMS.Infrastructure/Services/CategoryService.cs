using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using System.Linq.Expressions;
using KSMS.Application.Extensions;
using KSMS.Domain.Dtos.Requests.Categorie;
using Microsoft.EntityFrameworkCore;
using Mapster;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services
{
    public class CategoryService : BaseService<CategoryService>, ICategoryService
    {
        
        public CategoryService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<CategoryService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        public async Task CreateCompetitionCategory(CreateCompetitionCategoryRequest request)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.ShowId);
            if (show == null)
            {
                throw new NotFoundException("Show not found");
            }

            if (request.CreateCompetionCategoryVarieties.Any())
            {
                foreach (var varietyId in request.CreateCompetionCategoryVarieties)
                {
                    var variety = await _unitOfWork.GetRepository<Variety>()
                        .SingleOrDefaultAsync(predicate: x => x.Id == varietyId);
                    if (variety == null)
                    {
                        throw new NotFoundException("Variety with Id:"  + varietyId + " not found");
                    }
                }
            }

            if (request.CreateCriteriaCompetitionCategoryRequests.Any())
            {
                foreach (var criteriaCategory in request.CreateCriteriaCompetitionCategoryRequests)
                {
                    var criteria = await _unitOfWork.GetRepository<Criterion>().SingleOrDefaultAsync(predicate: x => x.Id == criteriaCategory.CriteriaId);
                    if (criteria == null)
                    {
                        throw new NotFoundException("Criteria with Id:" + criteriaCategory.CriteriaId + " not found");
                    }
                }
            }
            var category = request.Adapt<CompetitionCategory>();
            await _unitOfWork.GetRepository<CompetitionCategory>().InsertAsync(category);
            await _unitOfWork.CommitAsync();
            if (request.CreateRoundRequests.Any())
            {
                var rounds = request.CreateRoundRequests.Select(r =>
                {
                    var round = r.Adapt<Round>();
                    round.CompetitionCategoriesId = category.Id;
                    return round;
                }).ToList();
                await _unitOfWork.GetRepository<Round>().InsertRangeAsync(rounds);
            }
            if (request.CreateCompetionCategoryVarieties.Any())
            {
                var categoryVarieties = request.CreateCompetionCategoryVarieties.Select(v => new CategoryVariety
                {
                    CompetitionCategoryId = category.Id,
                    VarietyId = v
                }).ToList();
                await _unitOfWork.GetRepository<CategoryVariety>().InsertRangeAsync(categoryVarieties);
            }
            if (request.CreateAwardCateShowRequests.Any())
            {
                var awards = request.CreateAwardCateShowRequests.Select(a =>
                {
                    var award = a.Adapt<Award>();
                    award.CompetitionCategoriesId = category.Id;
                    return award;
                }).ToList();
                await _unitOfWork.GetRepository<Award>().InsertRangeAsync(awards);
            }
            if (request.CreateCriteriaCompetitionCategoryRequests.Any())
            {
                var criteriaCategories = request.CreateCriteriaCompetitionCategoryRequests.Select(c =>
                {
                    var criteria = c.Adapt<CriteriaCompetitionCategory>();
                    criteria.CompetitionCategoryId = category.Id;
                    return criteria;
                }).ToList();
                await _unitOfWork.GetRepository<CriteriaCompetitionCategory>().InsertRangeAsync(criteriaCategories);
            }
            if (request.CreateRefereeAssignmentRequests.Any())
            {
                var refereeAssignments = request.CreateRefereeAssignmentRequests
                    .SelectMany(r => r.RoundTypes.Select(rt => new RefereeAssignment
                    {
                        RefereeAccountId = r.RefereeAccountId,
                        CompetitionCategoryId = category.Id,
                        RoundType = rt,
                        AssignedAt = DateTime.UtcNow,
                        AssignedBy = GetIdFromJwt()
                    })).ToList();
                await _unitOfWork.GetRepository<RefereeAssignment>().InsertRangeAsync(refereeAssignments);
            }

            await _unitOfWork.CommitAsync();
        }

        public async Task<GetCompetitionCategoryDetailResponse> GetCompetitionCategoryDetailById(Guid id)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate:
                x => x.Id == id,
                include: query => query
                    .Include(x => x.Awards)   
                    .Include(x => x.CategoryVarieties)
                        .ThenInclude(x => x.Variety)
                    .Include(x => x.CriteriaCompetitionCategories)
                        .ThenInclude(x => x.Criteria)
                    .Include(x => x.RefereeAssignments)
                        .ThenInclude(x => x.RefereeAccount)
                    .Include(x => x.RefereeAssignments)
                        .ThenInclude(x => x.AssignedByNavigation));
            if (category == null)
            {
                throw new NotFoundException("Category not found");
            }
            return category.Adapt<GetCompetitionCategoryDetailResponse>();
        }

        public async Task<Paginate<GetPageCompetitionCategoryResponse>> GetPagedCompetitionCategory(Guid showId, int page, int size)
        {
            var role = GetRoleFromJwt();
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == showId);
            if (show == null)
            {
                throw new NotFoundException("Show not found");
            }
            Expression<Func<CompetitionCategory, bool>> filterQuery = category => category.KoiShowId == showId;
            // if (role is "Guest" or "Member")
            // {
            //     filterQuery = filterQuery.And(category => category.Status == CategoryStatus.Active.ToString().ToLower());
            // }
            if (role is "Referee")
            {
                filterQuery = filterQuery.AndAlso(category => category.RefereeAssignments.Any(x => x.RefereeAccountId == GetIdFromJwt()));
            }
            var categories = await _unitOfWork.GetRepository<CompetitionCategory>().GetPagingListAsync(predicate:
                filterQuery,
                orderBy: query => query.OrderBy(x => x.Name),
                include: query => query
                    .Include(x => x.RefereeAssignments),
                page: page,
                size: size
            );
            return categories.Adapt<Paginate<GetPageCompetitionCategoryResponse>>();
        }
        
        
    }
}
