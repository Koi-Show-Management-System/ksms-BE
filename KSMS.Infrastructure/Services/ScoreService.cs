using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Dtos.Responses.Score;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

using Mapster;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Linq;
using KSMS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KSMS.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using KSMS.Infrastructure.Hubs;

namespace KSMS.Infrastructure.Services
{
    public class ScoreService : BaseService<ScoreService>, IScoreService
    {
        private readonly IHubContext<ScoreHub> _scoreHub;

        public ScoreService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ScoreService> logger, IHttpContextAccessor httpContextAccessor, IHubContext<ScoreHub> scoreHub) : base(unitOfWork, logger, httpContextAccessor)
        {
            _scoreHub = scoreHub;
        }
        public async Task<Paginate<ScoreDetailResponse>> GetPagedScoresAsync(int page, int size)
        {
            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();

            var pagedScores = await scoreRepository.GetPagingListAsync(
                orderBy: query => query.OrderBy(s => s.RegistrationRoundId),
                include: query => query
                    .Include(s => s.ScoreDetailErrors)
                    .Include(s => s.RegistrationRound)
                        .ThenInclude(s => s.Round),
                page: page,
                size: size
            );

            return pagedScores.Adapt<Paginate<ScoreDetailResponse>>();
        }

        public async Task<ScoreDetailResponse> CreateScoreAsync(CreateScoreDetailRequest request)
        {
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
            var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
            var roundresultRepository = _unitOfWork.GetRepository<RoundResult>();

            var referee = await _unitOfWork.GetRepository<RefereeAssignment>()
                            .SingleOrDefaultAsync(r => r.Id == request.RefereeAccountId, null, null);


            if (referee == null )
            {
                throw new UnauthorizedException("Only referees can create scores.");
            }

            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
                .SingleOrDefaultAsync(r => r.Id == request.RegistrationRoundId, null, null);

            if (registrationRound == null)
            {
                throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
            }

            
            var score = request.Adapt<ScoreDetail>();
            score.Id = Guid.NewGuid();
            await scoreRepository.InsertAsync(score);
            var scoreid = score.Id;
            if (request.CreateScoreDetailErrors.Any())
            {
                foreach (var error in request.CreateScoreDetailErrors)
                {
                    var scoreDetailError = new ScoreDetailError
                    {
                        Id = Guid.NewGuid(),   
                        ScoreDetailId = scoreid,   
                        ErrorTypeId = error.ErrorTypeId,
                        Severity = error.Severity,
                        PointMinus = error.PointMinus,
                        CreatedAt = DateTime.UtcNow
                    };

                    await scoreDetailErrorRepository.InsertAsync(scoreDetailError);  // Lưu ScoreDetailError
                }
            }

            // Tính toán tổng điểm
            var totalScore = 100 - score.TotalPointMinus;

            // Tạo và lưu RoundResult
            var roundResult = new RoundResult
            {
                RegistrationRoundsId = request.RegistrationRoundId,
                TotalScore = totalScore,
                CreatedAt = DateTime.UtcNow
            };

            await roundresultRepository.InsertAsync(roundResult);

           
            await _unitOfWork.CommitAsync();

             
            await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores");

           
            return score.Adapt<ScoreDetailResponse>();
        }


    }
}
