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
using KSMS.Domain.Dtos.Responses.Round;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;

namespace KSMS.Infrastructure.Services
{
    public class RoundService : BaseService<RoundService>, IRoundService
    {
        public RoundService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<RoundService> logger, IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor)
        {
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
                throw new NotFoundException("Không tìm thấy vòng thi.");
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

        public async Task<Paginate<GetPageRoundResponse>> GetPageRound(Guid competitionCategoryId, RoundEnum roundType, int page, int size)
        {
            var competitionCategory = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == competitionCategoryId);
            if (competitionCategory == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục thi đấu");
            }
            var rounds = await _unitOfWork.GetRepository<Round>().GetPagingListAsync(
                predicate: x => x.CompetitionCategoriesId == competitionCategoryId && x.RoundType == roundType.ToString(),
                page: page,
                size: size
            );
            return rounds.Adapt<Paginate<GetPageRoundResponse>>();
        }

        public async Task<List<string>> GetRoundTypeForReferee(Guid competitionCategoryId)
        {
            var refereeAssignment = await _unitOfWork.GetRepository<RefereeAssignment>()
                .GetListAsync(predicate:
                    r => r.RefereeAccountId == GetIdFromJwt() && r.CompetitionCategoryId == competitionCategoryId);
            if (!refereeAssignment.Any())
            {
                return [];
            }
            var roundTypes = refereeAssignment.Select(r => r.RoundType).Distinct().ToList();
            return roundTypes;
        }

        public async Task<object> GetNextRound(Guid roundId)
        {
            var round = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(
                predicate: x => x.Id == roundId);
            if (round == null)
            {
                throw new NotFoundException("Không tìm thấy vòng thi");
            }
            var nextRound = await _unitOfWork.GetRepository<Round>()
                .SingleOrDefaultAsync(
                    predicate: r => r.CompetitionCategoriesId == round.CompetitionCategoriesId 
                                    && ((r.RoundType == round.RoundType && r.RoundOrder > round.RoundOrder)
                                        || (round.RoundType == RoundEnum.Preliminary.ToString()
                                            && r.RoundType == RoundEnum.Evaluation.ToString() && r.RoundOrder == 1)
                                        || (round.RoundType == RoundEnum.Evaluation.ToString() 
                                            && r.RoundType == RoundEnum.Final.ToString() && r.RoundOrder == 1)),
                    orderBy: q => q.OrderBy(r => r.RoundType != round.RoundType)
                        .ThenBy(r => r.RoundOrder)
                );
            if (nextRound == null)
            {
                throw new BadRequestException("Không tìm thấy vòng thi tiếp theo");
            }

            return new
            {
                NextRoundId = nextRound.Id,
            };
        }
    }
}