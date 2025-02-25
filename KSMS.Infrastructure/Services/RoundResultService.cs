using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RoundResult;
using KSMS.Domain.Dtos.Responses.RoundResult;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Services
{
    public class RoundResultService : IRoundResultService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;
        private readonly ILogger<RoundResultService> _logger;

        public RoundResultService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RoundResultService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RoundResultResponse> UpdateIsPublicAsync(Guid id, bool isPublic)
        {
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();

            
            var roundResult = await roundResultRepository.SingleOrDefaultAsync(rr => rr.Id == id, null,null);

            if (roundResult == null)
            {
                throw new NotFoundException("RoundResult not found");
            }

            
            roundResult.IsPublic = isPublic;

         
            roundResultRepository.UpdateAsync(roundResult);
            await _unitOfWork.CommitAsync();

            return roundResult.Adapt<RoundResultResponse>();
        }

        public async Task<RoundResultResponse> CreateRoundResultAsync(CreateRoundResult request)
        {
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();

                
            var existingRoundResult = await roundResultRepository.SingleOrDefaultAsync(
                predicate: rr => rr.RegistrationRoundsId == request.RegistrationRoundsId
            );

            if (existingRoundResult != null)
            {
                throw new BadRequestException("Round result already exists for this registration round.");
            }

            
            var roundResult = request.Adapt<RoundResult>();

            
            var createdRoundResult = await roundResultRepository.InsertAsync(roundResult);
            await _unitOfWork.CommitAsync();

             
            return createdRoundResult.Adapt<RoundResultResponse>();
        }
    }
}
