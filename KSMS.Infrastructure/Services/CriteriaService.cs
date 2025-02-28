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


        public async Task<CriteriaResponse> CreateCriteriaAsync(CreateCriteriaRequest createCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var errorTypeRepository = _unitOfWork.GetRepository<ErrorType>();

            var existingCriterion = await criterionRepository.SingleOrDefaultAsync(c => c.Name == createCriteriaRequest.Name, null, null);
            if (existingCriterion != null)
            {
                throw new BadRequestException($"Criterion with name '{createCriteriaRequest.Name}' already exists.");
            }


            var criterion = createCriteriaRequest.Adapt<Criterion>();


            if (createCriteriaRequest.CreateErrorTypeRequests != null && createCriteriaRequest.CreateErrorTypeRequests.Any())
            {
                foreach (var errorTypeRequest in createCriteriaRequest.CreateErrorTypeRequests)
                {
                    var errorType = errorTypeRequest.Adapt<ErrorType>();
                    criterion.ErrorTypes.Add(errorType);
                }
            }

            var createdCriterion = await criterionRepository.InsertAsync(criterion);
            await _unitOfWork.CommitAsync();

            return createdCriterion.Adapt<CriteriaResponse>();
        }


        public async Task<CriteriaResponse> GetCriteriaByIdAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(
           predicate: c => c.Id == id,
           include: query => query
               .Include(c => c.ErrorTypes));

            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }

            return criterion.Adapt<CriteriaResponse>();
        }

        public async Task<CriteriaResponse> UpdateCriteriaAsync(Guid id, UpdateCriteriaRequest updateCriteriaRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);
            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }

            updateCriteriaRequest.Adapt(criterion);


            if (updateCriteriaRequest.UpdateErrorTypeRequests != null && updateCriteriaRequest.UpdateErrorTypeRequests.Any())
            {
                foreach (var errorTypeRequest in updateCriteriaRequest.UpdateErrorTypeRequests)
                {
                    var existingErrorType = await _unitOfWork.GetRepository<ErrorType>()
                        .SingleOrDefaultAsync(e => e.Id == errorTypeRequest.CriteriaId, null, null);

                    if (existingErrorType == null)
                    {
                        var newErrorType = errorTypeRequest.Adapt<ErrorType>();
                        criterion.ErrorTypes.Add(newErrorType);
                    }
                }
            }

            criterionRepository.UpdateAsync(criterion);
            await _unitOfWork.CommitAsync();

            return criterion.Adapt<CriteriaResponse>();
        }

        public async Task<Paginate<GetAllCriteriaResponse>> GetPagingCriteria(int page, int size)
        {
            return (await _unitOfWork.GetRepository<Criterion>().GetPagingListAsync(page: page, size: size))
                .Adapt<Paginate<GetAllCriteriaResponse>>();
        }


        public async Task DeleteCriteriaAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id, null, null);

            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }

            // Remove related ErrorTypes if any
            foreach (var errorType in criterion.ErrorTypes)
            {
                _unitOfWork.GetRepository<ErrorType>().DeleteAsync(errorType);
            }

            // Delete the Criterion
            criterionRepository.DeleteAsync(criterion);
            await _unitOfWork.CommitAsync();
        }
    }
}