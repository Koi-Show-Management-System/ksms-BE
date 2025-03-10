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
using Microsoft.EntityFrameworkCore;
using KSMS.Domain.Pagination;
using KSMS.Domain.Dtos.Responses.Registration;
using KSMS.Domain.Enums;

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

        public async Task UpdateIsPublicByCategoryIdAsync(Guid categoryId, bool isPublic)
        {
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();

            
            var roundResults = await roundResultRepository.GetListAsync(
                predicate: rr => rr.RegistrationRounds.Registration.CompetitionCategoryId == categoryId,
                include: query => query.Include(rr => rr.RegistrationRounds)
                    .ThenInclude(rr => rr.Registration));

            if (roundResults == null || !roundResults.Any())
            {
                throw new NotFoundException("No RoundResults found for the provided CategoryId");
            }

           
            foreach (var result in roundResults)
            {
                result.IsPublic = isPublic;
                roundResultRepository.UpdateAsync(result);
            }

           
            await _unitOfWork.CommitAsync();

           
          
        }
        public async Task<Paginate<RegistrationGetByCategoryPagedResponse>> GetPagedRegistrationsByCategoryAndStatusAsync(Guid categoryId, RoundResultStatus? status, int page, int size)
        {
            var registrationRepository = _unitOfWork.GetRepository<Registration>();

            var statusString = status?.ToString();

            var pagedRegistrations = await registrationRepository.GetPagingListAsync(
                predicate: reg => reg.CompetitionCategoryId == categoryId &&
                    (status == null ||
                     reg.RegistrationRounds.Any(rr => rr.RoundResults.Any(rres => rres.Status == statusString))),
                include: query => query
                    .Include(reg => reg.KoiMedia)
                    .Include(reg => reg.CompetitionCategory)
                    .Include(reg => reg.RegistrationRounds)
                        .ThenInclude(rr => rr.Round)
                    .Include(reg => reg.RegistrationRounds)
                        .ThenInclude(rr => rr.RoundResults)
                    .Include(reg => reg.RegistrationRounds)
                        .ThenInclude(rr => rr.ScoreDetails)
                         .ThenInclude(sd => sd.RefereeAccount),
                orderBy: query => query
                    .OrderBy(reg => reg.CompetitionCategory.Name)
                    .ThenBy(reg => reg.CreatedAt),
                page: page,
                size: size
            );

            return pagedRegistrations.Adapt<Paginate<RegistrationGetByCategoryPagedResponse>>();
        }






        //public async Task<RoundResultResponse> CreateRoundResultAsync(CreateRoundResult request)
        //{
        //    var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();


        //    var existingRoundResult = await roundResultRepository.SingleOrDefaultAsync(
        //        predicate: rr => rr.RegistrationRoundsId == request.RegistrationRoundsId
        //    );

        //    if (existingRoundResult != null)
        //    {
        //        throw new BadRequestException("Round result already exists for this registration round.");
        //    }


        //    var roundResult = request.Adapt<RoundResult>();


        //    var createdRoundResult = await roundResultRepository.InsertAsync(roundResult);
        //    await _unitOfWork.CommitAsync();


        //    return createdRoundResult.Adapt<RoundResultResponse>();
        //}
    }
}
