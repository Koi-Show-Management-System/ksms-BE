using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace KSMS.Infrastructure.Services
{
    public class RoundService : IRoundService
    {
        private readonly IUnitOfWork<KoiShowManagementSystemContext> _unitOfWork;
        private readonly ILogger<RoundService> _logger;

        public RoundService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RoundService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
       

        public async Task UpdateRoundStatusAsync(Guid roundId)
        {
            var roundRepository = _unitOfWork.GetRepository<Round>();

           
            var roundToActivate = await roundRepository.SingleOrDefaultAsync(
                predicate: r => r.Id == roundId,
                include: query => query.Include(r => r.CompetitionCategories)
            );

            if (roundToActivate == null)
            {
                throw new NotFoundException("Round not found.");
            }

            var categoryId = roundToActivate.CompetitionCategoriesId;

           
            var activeRound = await roundRepository.SingleOrDefaultAsync(
                predicate: r => r.CompetitionCategoriesId == categoryId && r.Status == "Active"
            );

            
            if (activeRound != null)
            {
                activeRound.Status = "Off";
                roundRepository.UpdateAsync(activeRound);
            }

            
            roundToActivate.Status = "Active";
            roundRepository.UpdateAsync(roundToActivate);

            await _unitOfWork.CommitAsync();

            
        }

    }
}
