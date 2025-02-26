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
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace KSMS.Infrastructure.Services
{
    public class CriterionService : BaseService<CriterionService>, ICriterionService
    {
        public CriterionService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<CriterionService> logger, IHttpContextAccessor httpContextAccessor)
          : base(unitOfWork, logger, httpContextAccessor)
        {
        }

        
        public async Task<CriterionResponse> CreateCriterionAsync(CreateCriterionRequest createCriterionRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var errorTypeRepository = _unitOfWork.GetRepository<ErrorType>();

            var existingCriterion = await criterionRepository.SingleOrDefaultAsync(c => c.Name == createCriterionRequest.Name ,null,null);
            if (existingCriterion != null)
            {
                throw new BadRequestException($"Criterion with name '{createCriterionRequest.Name}' already exists.");
            }

           
            var criterion = createCriterionRequest.Adapt<Criterion>();

            
            if (createCriterionRequest.CreateErrorTypeRequests != null && createCriterionRequest.CreateErrorTypeRequests.Any())
            {
                foreach (var errorTypeRequest in createCriterionRequest.CreateErrorTypeRequests)
                {
                    var errorType = errorTypeRequest.Adapt<ErrorType>();
                    criterion.ErrorTypes.Add(errorType);
                    
                }
            }

            var createdCriterion = await criterionRepository.InsertAsync(criterion);
            await _unitOfWork.CommitAsync();

            return createdCriterion.Adapt<CriterionResponse>();
        }

  
        public async Task<CriterionResponse> GetCriterionByIdAsync(Guid id)
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

            return criterion.Adapt<CriterionResponse>();
        }

        public async Task<CriterionResponse> UpdateCriterionAsync(Guid id, UpdateCriterionRequest updateCriterionRequest)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();

            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id,null,null);
            if (criterion == null)
            {
                throw new NotFoundException("Criterion not found");
            }

            updateCriterionRequest.Adapt(criterion);

     
            if (updateCriterionRequest.UpdateErrorTypeRequests != null && updateCriterionRequest.UpdateErrorTypeRequests.Any())
            {
                foreach (var errorTypeRequest in updateCriterionRequest.UpdateErrorTypeRequests)
                {
                    var existingErrorType = await _unitOfWork.GetRepository<ErrorType>()
                        .SingleOrDefaultAsync(e => e.Name == errorTypeRequest.Name ,null,null);

                    if (existingErrorType == null)
                    {
                        var newErrorType = errorTypeRequest.Adapt<ErrorType>();
                        criterion.ErrorTypes.Add(newErrorType);
                    }
                }
            }

            criterionRepository.UpdateAsync(criterion);
            await _unitOfWork.CommitAsync();

            return criterion.Adapt<CriterionResponse>();
        }

      
        public async Task DeleteCriterionAsync(Guid id)
        {
            var criterionRepository = _unitOfWork.GetRepository<Criterion>();
            var criterion = await criterionRepository.SingleOrDefaultAsync(c => c.Id == id,null,null);

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
