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
using KSMS.Domain.Dtos.Requests.ScoreDetail;

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
                     .Include(s => s.RegistrationRound),
                page: page,
                size: size
            );

            return pagedScores.Adapt<Paginate<ScoreDetailResponse>>();
        }

        public async Task CreateScoreAsync(CreateScoreDetailRequest request)
        {
            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
            var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();

            //  Kiểm tra vòng thi có tồn tại
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
                include: query => query.Include(x => x.Registration).ThenInclude(x => x.CompetitionCategory)
                .Include(x => x.Round));

            if (registrationRound == null)
            {
                throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
            }

            //  Kiểm tra trọng tài có quyền chấm điểm vòng này
            var referee = await _unitOfWork.GetRepository<RefereeAssignment>()
                .SingleOrDefaultAsync(predicate: r => r.RefereeAccountId == request.RefereeAccountId);

            if (referee == null)
            {
                throw new ForbiddenMethodException("Forbidden");
            }

            //  Kiểm tra nếu trọng tài này đã chấm điểm vòng này
            var existingScore = await scoreRepository.SingleOrDefaultAsync(
                predicate: x => x.RefereeAccountId == request.RefereeAccountId &&
                                x.RegistrationRoundId == request.RegistrationRoundId);

            if (existingScore != null)
            {
                throw new BadRequestException("This referee has already scored this round.");
            }

            //  Tạo `ScoreDetail`
            var score = request.Adapt<ScoreDetail>();
            score.Id = Guid.NewGuid();
            await scoreRepository.InsertAsync(score);

            //  Thêm `ScoreDetailError` nếu có lỗi
            if (request.CreateScoreDetailErrors != null && request.CreateScoreDetailErrors.Any())
            {
                foreach (var error in request.CreateScoreDetailErrors)
                {
                    var scoreDetailError = new ScoreDetailError
                    {
                        ScoreDetailId = score.Id,
                        ErrorTypeId = error.ErrorTypeId,
                        Severity = error.Severity,
                        PointMinus = error.PointMinus
                    };

                    await scoreDetailErrorRepository.InsertAsync(scoreDetailError);
                }
            }

            //  Lấy danh sách tất cả điểm bị trừ
            var allScores = await scoreRepository.GetListAsync(
                predicate: s => s.RegistrationRoundId == request.RegistrationRoundId);

            var penaltyPoints = allScores.Select(s => s.TotalPointMinus).ToList();
            var totalReferees = penaltyPoints.Count;
           
            decimal finalScore;
            if (totalReferees == 0)
            {
                finalScore = 100 - request.TotalPointMinus;
            }
            else
            {
                penaltyPoints.Add(request.TotalPointMinus);
             
                ////  loại bỏ điểm cao nhất và thấp nhất nếu còn ít nhất 3 giá trị sau khi loại
                //if (penaltyPoints.Count > 3)
                //{
                //    penaltyPoints.Sort();
                //    penaltyPoints.RemoveAt(0); // Bỏ điểm thấp nhất
                //    penaltyPoints.RemoveAt(penaltyPoints.Count - 1); // Bỏ điểm cao nhất
                //}

                Console.WriteLine($"Sau khi loại bỏ: {string.Join(", ", penaltyPoints)}");

                // Nếu danh sách còn ít hơn 2 giá trị, giữ nguyên tất cả điểm
                var validReferees = penaltyPoints.Count > 1 ? penaltyPoints.Count : 1;
                  finalScore = 100 - (penaltyPoints.Sum() / validReferees);

            }

            //  Xác định `Pass` hoặc `Fail`
            decimal passThreshold = 50;
            //decimal passThreshold = (decimal)registrationRound.Round.MinScoreToAdvance;
            string finalStatus = finalScore >= passThreshold ? "Pass" : "Fail"; 
             

            // Cập nhật `RoundResult`
            var existingRoundResult = await roundResultRepository.SingleOrDefaultAsync(
                predicate: r => r.RegistrationRoundsId == request.RegistrationRoundId);

            if (existingRoundResult != null)
            {
                existingRoundResult.TotalScore = finalScore;
                existingRoundResult.Status = finalStatus;
                 
                 roundResultRepository.UpdateAsync(existingRoundResult);
            }
            else
            {
                var roundResult = new RoundResult
                {
                    RegistrationRoundsId = request.RegistrationRoundId,
                    TotalScore = finalScore,
                    Status = finalStatus,
                    IsPublic = false
                };

                await roundResultRepository.InsertAsync(roundResult);
            }

            await _unitOfWork.CommitAsync();

            //  Gửi cập nhật điểm số qua SignalR

            await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores", request.RegistrationRoundId, finalStatus);

         //   return score.Adapt<ScoreDetailResponse>();
        }

        public async Task CreateEliminationScoreAsync(CreateEliminationScoreRequest request)
        {
            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();

            //  Kiểm tra vòng thi có tồn tại
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
                                      include: query => query.Include(x => x.Registration)
                                                             .ThenInclude(x => x.CompetitionCategory)
                                                             .Include(x => x.Round));

            if (registrationRound == null)
            {
                throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
            }

            //  Tạo mới `ScoreDetail` cho trọng tài hiện tại
            var newScore = new ScoreDetail
            {
                RefereeAccountId = request.RefereeAccountId,
                RegistrationRoundId = request.RegistrationRoundId,
                TotalPointMinus = request.IsPass ? 0 : 100,
                IsPublic = false,
                InitialScore = 100
            };

            await scoreRepository.InsertAsync(newScore);
            await _unitOfWork.CommitAsync();


            // Lấy danh sách tất cả phiếu bầu của cá này trong vòng thi
            var allVotes = await scoreRepository.GetListAsync(
                predicate: s => s.RegistrationRoundId == request.RegistrationRoundId);
            
            int totalVotes = allVotes.Count;
            int totalPass = allVotes.Count(s => s.TotalPointMinus == 0); // Đếm số trọng tài chọn `Pass`

            //  Tính tỷ lệ pass
            decimal passRate = totalVotes > 0 ? (decimal)totalPass / totalVotes * 100 : 0;
            Console.WriteLine($"passrate : {string.Join(", ", passRate)}");
            //  Xác định điểm số cuối cùng
            const decimal PASS_THRESHOLD = 50; // Nếu ≥ 50% phiếu "Pass" → Giữ 100 điểm, ngược lại 0 điểm
            decimal finalScore = passRate >= PASS_THRESHOLD ? 100 : 0;

            //  Cập nhật `RoundResult`
            var existingRoundResult = await roundResultRepository.SingleOrDefaultAsync(
                predicate: r => r.RegistrationRoundsId == request.RegistrationRoundId);

            if (existingRoundResult != null)
            {
                existingRoundResult.TotalScore = finalScore;
                existingRoundResult.Status = finalScore > 0 ? "Pass" : "Fail";
                roundResultRepository.UpdateAsync(existingRoundResult);
            }
            else
            {
                var roundResult = new RoundResult
                {
                    RegistrationRoundsId = request.RegistrationRoundId,
                    TotalScore = finalScore,
                    IsPublic = false,
                    Status = finalScore > 0 ? "Pass" : "Fail"
                };

                await roundResultRepository.InsertAsync(roundResult);
            }

            await _unitOfWork.CommitAsync();
            await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedRoundStatus", request.RegistrationRoundId, finalScore);
        }


    }
}
