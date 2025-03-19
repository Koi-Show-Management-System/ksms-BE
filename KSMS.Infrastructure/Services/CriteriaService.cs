using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Criterion;
using KSMS.Domain.Dtos.Responses.Criterion;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using Mapster;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KSMS.Domain.Dtos.Responses.CriteriaCompetitionCategory;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KSMS.Infrastructure.Services
{
    public class CriteriaService : BaseService<CriteriaService>, ICriterionService
    {
        public CriteriaService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<CriteriaService> logger, IHttpContextAccessor httpContextAccessor)
          : base(unitOfWork, logger, httpContextAccessor)
        {
        }


        public async Task CreateCriteriaAsync(CreateCriteriaRequest createCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var existingCriterion = await criterionRepository.SingleOrDefaultAsync(c => c.Name == createCriteriaRequest.Name, null, null);
            if (existingCriterion != null)
            {
                throw new BadRequestException($"Criterion with name '{createCriteriaRequest.Name}' already exists.");
            }
            await criterionRepository.InsertAsync(createCriteriaRequest.Adapt<Criterion>());
            await _unitOfWork.CommitAsync();

        }


        public async Task<CriteriaResponse> GetCriteriaByIdAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(
                predicate: c => c.Id == id);

            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }

            return criterion.Adapt<CriteriaResponse>();
        }

        public async Task UpdateCriteriaAsync(Guid id, UpdateCriteriaRequest updateCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);
            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }
            updateCriteriaRequest.Adapt(criterion);

            criterionRepository.UpdateAsync(criterion);
            await _unitOfWork.CommitAsync();
        }

        public async Task<Paginate<GetAllCriteriaResponse>> GetPagingCriteria(int page, int size)
        {
            return (await _unitOfWork.GetRepository<Criterion>().GetPagingListAsync(page: page, size: size))
                .Adapt<Paginate<GetAllCriteriaResponse>>();
        }

        public async Task<List<GetCriteriaCompetitionCategoryResponse>> GetCriteriaCompetitionCategory(Guid competitionCategoryId, RoundEnum roundType)
        {
            var criteriaCompetition = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                .GetListAsync(
                    predicate: x => x.CompetitionCategoryId == competitionCategoryId &&
                                    x.RoundType == roundType.ToString(),
                    include: query =>
                        query.Include(x => x.Criteria));
            return criteriaCompetition.Adapt<List<GetCriteriaCompetitionCategoryResponse>>();
        }


        public async Task DeleteCriteriaAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);

            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }
            criterionRepository.DeleteAsync(criterion);
            await _unitOfWork.CommitAsync();
        }
    }
}