//    using KSMS.Application.Repositories;
//using KSMS.Application.Services;
//using KSMS.Domain.Dtos.Requests.Score;
//using KSMS.Domain.Dtos.Responses;
//using KSMS.Domain.Dtos.Responses.Score;
//using KSMS.Domain.Entities;
//using KSMS.Domain.Enums;
//using KSMS.Domain.Pagination;

//using Mapster;
//using Microsoft.AspNetCore.SignalR;
//using System;
//using System.Threading.Tasks;
//using System.Linq;
//using KSMS.Domain.Exceptions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using KSMS.Infrastructure.Database;
//using Microsoft.AspNetCore.Http;
//using KSMS.Infrastructure.Hubs;

//namespace KSMS.Infrastructure.Services
//{
//    public class ScoreService : BaseService<ScoreService>, IScoreService
//    {
//        private readonly IHubContext<ScoreHub> _scoreHub;

//        public ScoreService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ScoreService> logger, IHttpContextAccessor httpContextAccessor, IHubContext<ScoreHub> scoreHub) : base(unitOfWork, logger, httpContextAccessor)
//        {
//            _scoreHub = scoreHub;
//        }
//        public async Task<Paginate<ScoreDetailResponse>> GetPagedScoresAsync(int page, int size)
//        {
//            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();

//            var pagedScores = await scoreRepository.GetPagingListAsync(
//                orderBy: query => query.OrderBy(s => s.RegistrationRoundId),
//                include: query => query
//                    .Include(s => s.ScoreDetailErrors)
//                    .Include(s => s.RegistrationRound)
//                        .ThenInclude(s => s.Round),
//                page: page,
//                size: size
//            );

//            return pagedScores.Adapt<Paginate<ScoreDetailResponse>>();
//        }

//        //public async Task<ScoreDetailResponse> CreateScoreAsync(CreateScoreDetailRequest request)
//        //{
//        //    var accountId = GetIdFromJwt();
//        //    var accountRepository = _unitOfWork.GetRepository<Account>();
//        //    var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
//        //    var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
//        //    var roundresultRepository = _unitOfWork.GetRepository<RoundResult>();
//        //    var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();

//        //    // Tìm RefereeAssignment bằng RefereeAccountId
            
//        //    var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
//        //        .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
//        //        include: query => query.Include(x => x.Registration).ThenInclude(x => x.CompetitionCategory)
//        //        .Include(x => x.Round ));
//        //    var referee = await _unitOfWork.GetRepository<RefereeAssignment>()
//        //                    .SingleOrDefaultAsync(predicate:r => r.RefereeAccountId == accountId &&
//        //                    r.RoundType == registrationRound.Round.RoundType
//        //                    && r.CompetitionCategoryId == registrationRound.Registration.C);

//        //    if (referee == null)
//        //    {
//        //        throw new ForbiddenMethodException("Forbidden");
//        //    }
//        //    // Đếm số lượng RefereeAssignmentId
//        //    var refereeAssignmentsCount = await refereeAssignmentRepository
//        //        .CountAsync(r => r. == request.RefereeAccountId);

//        //    if (refereeAssignmentsCount == 0)
//        //    {
//        //        throw new NotFoundException($"No referee assignments found for RefereeAccountId '{request.RefereeAccountId}'.");
//        //    }

            

//        //    var score = request.Adapt<ScoreDetail>();
//        //    score.Id = Guid.NewGuid();
//        //    await scoreRepository.InsertAsync(score);

//        //    var scoreId = score.Id;
//        //    if (request.CreateScoreDetailErrors != null && request.CreateScoreDetailErrors.Any())
//        //    {
//        //        foreach (var error in request.CreateScoreDetailErrors)
//        //        {
//        //            var scoreDetailError = new ScoreDetailError
//        //            {
//        //                Id = Guid.NewGuid(),
//        //                ScoreDetailId = scoreId,
//        //                ErrorTypeId = error.ErrorTypeId,
//        //                Severity = error.Severity,
//        //                PointMinus = error.PointMinus,
//        //            //    CreatedAt = DateTime.UtcNow
//        //            };

//        //            await scoreDetailErrorRepository.InsertAsync(scoreDetailError);
//        //        }
//        //    }

//        //    // Lấy thông tin RegistrationRound
//        //    //var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
//        //    //    .SingleOrDefaultAsync(r => r.Id == request.RegistrationRoundId, null, null);

//        //    //if (registrationRound == null)
//        //    //{
//        //    //    throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
//        //    //}
//        //    var totalScore = (100 - score.TotalPointMinus) / refereeAssignmentsCount;

//        //    var roundResult = new RoundResult
//        //    {
//        //        RegistrationRoundsId = request.RegistrationRoundId,
//        //        TotalScore = totalScore,
//        //        CreatedAt = DateTime.UtcNow
//        //    };

//        //    await roundresultRepository.InsertAsync(roundResult);

          
//        //    await _unitOfWork.CommitAsync();

           
//        //    await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores");

           
//        //    return score.Adapt<ScoreDetailResponse>();
//        //}

//    }
//}
