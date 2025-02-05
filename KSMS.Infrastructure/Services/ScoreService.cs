using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Score;
using KSMS.Domain.Dtos.Responses;
using KSMS.Domain.Dtos.Responses.Score;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.SignalR;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using System.Linq;
using KSMS.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using KSMS.Infrastructure.Database;

namespace KSMS.Infrastructure.Services
{
    public class ScoreService : BaseService<ScoreService> ,  IScoreService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ScoreHub> _scoreHub;
        public ScoreService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, IHubContext<ScoreHub> scoreHub, ILogger<ScoreService> logger)
         : base(unitOfWork, logger)  
        {
            _unitOfWork = unitOfWork;
            _scoreHub = scoreHub;
        }
        public async Task<Paginate<ScoreResponse>> GetPagedScoresAsync(int page, int size)
        {
            var scoreRepository = _unitOfWork.GetRepository<Score>();

            var pagedScores = await scoreRepository.GetPagingListAsync(
                orderBy: query => query.OrderBy(s => s.RegistrationId), 
                include: query => query
                    .Include(s => s.Criteria)
                    .Include(s => s.RefereeAccount)
                    .Include(s => s.Registration)
                    .Include(s => s.Round),
                page: page,
                size: size
            );

            return pagedScores.Adapt<Paginate<ScoreResponse>>();
        }

        public async Task<ScoreResponse> CreateScoreAsync(CreateScoreRequest request)
        {
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var scoreRepository = _unitOfWork.GetRepository<Score>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();

            var referee = await accountRepository.SingleOrDefaultAsync(
                    predicate:  a => a.Id == request.RefereeAccountId,
                      include: query => query.Include(a => a.Role)
                      );
            if (referee == null || referee.Role.Name != "Referee")
            {
                throw new UnauthorizedException("Only referees can create scores.");
            }


            var criteria = await criteriaRepository.SingleOrDefaultAsync(c => c.Id == request.CriteriaId, null, null);
            if (criteria == null)
            {
                throw new NotFoundException($"Criteria with ID '{request.CriteriaId}' not found.");
            }
            var score = request.Adapt<Score>();
           

            await scoreRepository.InsertAsync(score);
            await _unitOfWork.CommitAsync();


            await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores");

            return score.Adapt<ScoreResponse>();
        }
    }
}
