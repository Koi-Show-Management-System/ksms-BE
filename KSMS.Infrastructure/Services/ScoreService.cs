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
        private readonly ICacheService _cacheService;
        public ScoreService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<ScoreService> logger, IHttpContextAccessor httpContextAccessor, IHubContext<ScoreHub> scoreHub, ICacheService cacheService) : base(unitOfWork, logger, httpContextAccessor)
        {
            _scoreHub = scoreHub;
            _cacheService = cacheService;
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

        //public async Task CreateScoreAsync(CreateScoreDetailRequest request)
        //{
        //    var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
        //    var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
        //    var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();
        //    var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();

        //    //  Kiểm tra vòng thi có tồn tại
        //    var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
        //        .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
        //        include: query => query.Include(x => x.Registration).ThenInclude(x => x.CompetitionCategory)
        //        .Include(x => x.Round));

        //    if (registrationRound == null)
        //    {
        //        throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
        //    }

        //    //  Kiểm tra trọng tài có quyền chấm điểm vòng này
        //    var referee = await _unitOfWork.GetRepository<RefereeAssignment>()
        //        .SingleOrDefaultAsync(predicate: r => r.RefereeAccountId == request.RefereeAccountId);

        //    if (referee == null)
        //    {
        //        throw new ForbiddenMethodException("Forbidden");
        //    }

        //    //  Kiểm tra nếu trọng tài này đã chấm điểm vòng này
        //    var existingScore = await scoreRepository.SingleOrDefaultAsync(
        //        predicate: x => x.RefereeAccountId == request.RefereeAccountId &&
        //                        x.RegistrationRoundId == request.RegistrationRoundId);

        //    if (existingScore != null)
        //    {
        //        throw new BadRequestException("This referee has already scored this round.");
        //    }

        //    //  Tạo ScoreDetail
        //    var score = request.Adapt<ScoreDetail>();
        //    score.Id = Guid.NewGuid();
        //    await scoreRepository.InsertAsync(score);

        //    //  Thêm ScoreDetailError nếu có lỗi
        //    if (request.CreateScoreDetailErrors != null && request.CreateScoreDetailErrors.Any())
        //    {
        //        foreach (var error in request.CreateScoreDetailErrors)
        //        {
        //            var scoreDetailError = new ScoreDetailError
        //            {
        //                ScoreDetailId = score.Id,
        //                ErrorTypeId = error.ErrorTypeId,
        //                Severity = error.Severity,
        //                PointMinus = error.PointMinus
        //            };

        //            await scoreDetailErrorRepository.InsertAsync(scoreDetailError);
        //        }
        //    }

        //    //  Lấy danh sách tất cả điểm bị trừ
        //    var allScores = await scoreRepository.GetListAsync(
        //        predicate: s => s.RegistrationRoundId == request.RegistrationRoundId);

        //    var penaltyPoints = allScores.Select(s => s.TotalPointMinus).ToList();
        //    var totalReferees = penaltyPoints.Count;

        //    decimal finalScore;
        //    if (totalReferees == 0)
        //    {
        //        finalScore = 100 - request.TotalPointMinus;
        //    }
        //    else
        //    {
        //        penaltyPoints.Add(request.TotalPointMinus);

        //        ////  loại bỏ điểm cao nhất và thấp nhất nếu còn ít nhất 3 giá trị sau khi loại
        //        //if (penaltyPoints.Count > 3)
        //        //{
        //        //    penaltyPoints.Sort();
        //        //    penaltyPoints.RemoveAt(0); // Bỏ điểm thấp nhất
        //        //    penaltyPoints.RemoveAt(penaltyPoints.Count - 1); // Bỏ điểm cao nhất
        //        //}

        //        Console.WriteLine($"Sau khi loại bỏ: {string.Join(", ", penaltyPoints)}");

        //        // Nếu danh sách còn ít hơn 2 giá trị, giữ nguyên tất cả điểm
        //        var validReferees = penaltyPoints.Count > 1 ? penaltyPoints.Count : 1;
        //          finalScore = 100 - (penaltyPoints.Sum() / validReferees);

        //    }

        //    //  Xác định Pass hoặc Fail
        //    decimal passThreshold = 50;
        //    //decimal passThreshold = (decimal)registrationRound.Round.MinScoreToAdvance;
        //    string finalStatus = finalScore >= passThreshold ? "Pass" : "Fail"; 


        //    // Cập nhật RoundResult
        //    var existingRoundResult = await roundResultRepository.SingleOrDefaultAsync(
        //        predicate: r => r.RegistrationRoundsId == request.RegistrationRoundId);

        //    if (existingRoundResult != null)
        //    {
        //        existingRoundResult.TotalScore = finalScore;
        //        existingRoundResult.Status = finalStatus;

        //         roundResultRepository.UpdateAsync(existingRoundResult);
        //    }
        //    else
        //    {
        //        var roundResult = new RoundResult
        //        {
        //            RegistrationRoundsId = request.RegistrationRoundId,
        //            TotalScore = finalScore,
        //            Status = finalStatus,
        //            IsPublic = false
        //        };

        //        await roundResultRepository.InsertAsync(roundResult);
        //    }

        //    await _unitOfWork.CommitAsync();

        //    //  Gửi cập nhật điểm số qua SignalR

        //    await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores", request.RegistrationRoundId, finalStatus);

        // //   return score.Adapt<ScoreDetailResponse>();
        //}
        public async Task CreateScoreAsync(CreateScoreDetailRequest request)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
                var scoreDetailErrorRepository = _unitOfWork.GetRepository<ScoreDetailError>();
                var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();

                // 1️⃣ Kiểm tra vòng thi có tồn tại
                var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
                    .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
                                          include: query => query.Include(x => x.Registration)
                                                                 .ThenInclude(x => x.CompetitionCategory)
                                                                 .Include(x => x.Round));

                if (registrationRound == null)
                {
                    throw new NotFoundException($"Không tìm thấy vòng đăng ký có ID '{request.RegistrationRoundId}'");
                } 
                var competitionCategoryId = registrationRound.Registration.CompetitionCategoryId;
                var roundType = registrationRound.Round.RoundType;

                //  Kiểm tra nếu hạng mục thi đấu *không phải* là "Preliminary" thì từ chối chấm điểm
                if (roundType == "Preliminary") // 🔥 Kiểm tra loại vòng đấu
                {
                    throw new BadRequestException($"Không được phép chấm điểm cho vòng loại");
                }
                // 2️ Kiểm tra trọng tài có quyền chấm điểm vòng này
                var referee = await refereeAssignmentRepository.SingleOrDefaultAsync(
                    predicate: r => r.RefereeAccountId == request.RefereeAccountId
                                    && r.CompetitionCategoryId == competitionCategoryId);

                if (referee == null)
                {
                    throw new ForbiddenMethodException("Trọng tài không được phân công cho hạng mục này");
                }

                // 3️ Kiểm tra nếu trọng tài này đã chấm điểm vòng này
                var existingScore = await scoreRepository.SingleOrDefaultAsync(
                    predicate: x => x.RefereeAccountId == request.RefereeAccountId &&
                                    x.RegistrationRoundId == request.RegistrationRoundId);

                if (existingScore != null)
                {
                    throw new BadRequestException("Trọng tài này đã chấm điểm cho vòng thi này");
                }

                // 4️⃣ Tạo ScoreDetail
                var score = request.Adapt<ScoreDetail>();
                score.Id = Guid.NewGuid();
                await scoreRepository.InsertAsync(score);
              
                // 🔥 5️⃣ Thêm ScoreDetailError nếu có lỗi
                if (request.CreateScoreDetailErrors?.Any() == true)
                {
                    var scoreDetailErrors = request.CreateScoreDetailErrors.Select(error => new ScoreDetailError
                    {
                        ScoreDetailId = score.Id,
                        ErrorTypeId = error.ErrorTypeId,
                        Severity = error.Severity,
                        PointMinus = error.PointMinus
                    }).ToList();

                    await scoreDetailErrorRepository.InsertRangeAsync(scoreDetailErrors);
                }

                await _unitOfWork.CommitAsync(); //  Lưu điểm của trọng tài ngay lập tức
                await transaction.CommitAsync();

                //   await _scoreHub.Clients.All.SendAsync("ReceiveUpdatedScores", request.RegistrationRoundId, score.TotalPointMinus);

                return;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Failed to process score: {ex.Message}");
            }
        }

        public async Task CreateEliminationScoreAsync(CreateEliminationScoreRequest request)
        {
            var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
            var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();

            // 1️⃣ Kiểm tra vòng thi có tồn tại
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
                                      include: query => query.Include(x => x.Registration)
                                                             .ThenInclude(x => x.CompetitionCategory)
                                                             .Include(x => x.Round));

            if (registrationRound == null)
            {
                throw new NotFoundException($"Không tìm thấy vòng đăng ký có ID '{request.RegistrationRoundId}'");
            }

            var competitionCategoryId = registrationRound.Registration.CompetitionCategoryId;
            var roundType = registrationRound.Round.RoundType;

            // 2️⃣ Kiểm tra nếu hạng mục thi đấu *không phải* là "Preliminary" thì từ chối chấm điểm
            if (roundType != "Preliminary") // 🔥 Kiểm tra loại vòng đấu
            {
                throw new BadRequestException($"Chỉ được phép chấm điểm cho vòng loại. Vòng hiện tại: '{roundType}'");
            }
            // 2️⃣ Kiểm tra trọng tài có quyền chấm điểm vòng này
            var referee = await refereeAssignmentRepository.SingleOrDefaultAsync(
                predicate: r => r.RefereeAccountId == request.RefereeAccountId
                                && r.CompetitionCategoryId == competitionCategoryId
                                && r.RoundType == registrationRound.Round.RoundType);

            if (referee == null)
            {
                throw new ForbiddenMethodException($"Bạn không được phân công cho hạng mục này trong vòng này");
            }

            // 3️⃣ Kiểm tra trọng tài đã chấm điểm chưa
            var existingScore = await scoreRepository.SingleOrDefaultAsync(
                predicate: s => s.RefereeAccountId == request.RefereeAccountId
                                && s.RegistrationRoundId == request.RegistrationRoundId);

            if (existingScore != null)
            {
                throw new BadRequestException($"Trọng tài '{request.RefereeAccountId}' đã chấm điểm cho vòng này");
            }

            // 4️⃣ Tạo mới ScoreDetail cho trọng tài hiện tại
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

            // 5️⃣ Lấy danh sách tất cả phiếu bầu của cá này trong vòng thi
            var allVotes = await scoreRepository.GetListAsync(
                predicate: s => s.RegistrationRoundId == request.RegistrationRoundId);

            int totalVotes = allVotes.Count;
            int totalPass = allVotes.Count(s => s.TotalPointMinus == 0); // Đếm số trọng tài chọn Pass

            // 6️⃣ Kiểm tra số lượng trọng tài đã chấm điểm
            int totalReferees = await refereeAssignmentRepository.CountAsync(
                predicate: r => r.RoundType == registrationRound.Round.RoundType
                                && r.CompetitionCategoryId == competitionCategoryId);

            //  Kiểm tra nếu không có trọng tài nào (tránh lỗi chia cho 0)
            if (totalReferees == 0)
            {
                throw new Exception($"Không có trọng tài nào được phân công cho hạng mục này trong vòng này");
            }

            bool isLastJudge = totalVotes >= totalReferees; // Nếu số phiếu chấm bằng số trọng tài → Trọng tài cuối cùng

            // 7️⃣ Chỉ tạo RoundResult nếu là trọng tài cuối cùng
            if (isLastJudge)
            {
                // 8️⃣ Tính tỷ lệ pass
                decimal passRate = (decimal)totalPass / totalVotes * 100;
                const decimal PASS_THRESHOLD = 50;
                decimal finalScore = passRate >= PASS_THRESHOLD ? 100 : 0;

                var roundResult = new RoundResult
                {
                    RegistrationRoundsId = request.RegistrationRoundId,
                    TotalScore = finalScore,
                    IsPublic = false,
                    Status = finalScore > 0 ? "Pass" : "Fail"
                };

                await roundResultRepository.InsertAsync(roundResult);

                // 🔥 Cần commit thay đổi trước khi gửi dữ liệu lên Hub
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<List<GetScoreDetailByRegistrationRoundResponse>> GetScoresByRegistrationRoundId(
            Guid registrationRoundId)
        {
            var accountId = GetIdFromJwt();
            var role = GetRoleFromJwt();
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>().SingleOrDefaultAsync(
                predicate: r => r.Id == registrationRoundId,
                include: query => query.AsSplitQuery()
                    .Include(r => r.Registration)
                        .ThenInclude(r => r.CompetitionCategory)
                    .Include(r => r.Round)
                    .Include(r => r.ScoreDetails)
                        .ThenInclude(r => r.RefereeAccount)
                    .Include(r => r.ScoreDetails)
                        .ThenInclude(r => r.ScoreDetailErrors)
                            .ThenInclude(r => r.ErrorType)
                                .ThenInclude(r => r.Criteria));
            if (registrationRound is null)
            {
                throw new NotFoundException("Không tìm thấy vòng thi");
            }
            var criteriaCompetitionCategories = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                .GetListAsync(predicate: x => x.CompetitionCategoryId == registrationRound.Registration.CompetitionCategoryId
                && x.RoundType == registrationRound.Round.RoundType);
            var scores = registrationRound.ScoreDetails;
            if (role == RoleName.Referee.ToString())
            {
                scores = scores.Where(s => s.RefereeAccountId == accountId).ToList();
            }

            var response = scores.Select(score =>
            {
                var scoreResponse = score.Adapt<GetScoreDetailByRegistrationRoundResponse>();
                if (registrationRound.Round.RoundType == RoundEnum.Preliminary.ToString())
                {
                    scoreResponse.Status = score.TotalPointMinus == 0 ? "Pass" : "Fail";
                    scoreResponse.TotalPointMinus = null;

                }
                var criteriaGroups = score.ScoreDetailErrors
                    .GroupBy(error => new
                    {
                        error.ErrorType.Criteria.Id, error.ErrorType.Criteria.Name,
                        error.ErrorType.Criteria.Description, error.ErrorType.Criteria.Order
                    })
                    .Select(group =>
                    {
                        var criteriaCompetitionCategory = criteriaCompetitionCategories
                            .FirstOrDefault(c => c.CriteriaId == group.Key.Id);
                        return new CriteriaWithErrorResponse
                        {
                            Id = group.Key.Id,
                            Name = group.Key.Name,
                            Description = group.Key.Description,
                            Order = criteriaCompetitionCategory?.Order,
                            Weight = criteriaCompetitionCategory?.Weight,
                            Errors = group.SelectMany(error => new[]
                            {
                                new ScoreDetailErrorWithTypeResponse
                                {
                                    Id = error.Id,
                                    ErrorTypeName = error.ErrorType.Name,
                                    Severity = error.Severity,
                                    PointMinus = error.PointMinus
                                }
                            }).ToList()
                        };
                    }).OrderBy(c => c.Order).ToList();
                scoreResponse.CriteriaWithErrors = criteriaGroups;
                return scoreResponse;
            }).OrderByDescending(s => s.CreatedAt).ToList();
            return response;
        }

        //public async Task CreateEliminationScoreAsync(CreateEliminationScoreRequest request)
        //{
        //    using var transaction = await _unitOfWork.BeginTransactionAsync();
        //    RegistrationRound? registrationRound = null;
        //    try
        //    {
        //        var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
        //        var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();
        //        var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
        //        var roundRepository = _unitOfWork.GetRepository<Round>();

        //        // 1️⃣ Kiểm tra vòng thi có tồn tại
        //          registrationRound = await _unitOfWork.GetRepository<RegistrationRound>()
        //            .SingleOrDefaultAsync(predicate: x => x.Id == request.RegistrationRoundId,
        //                                  include: query => query.Include(x => x.Registration)
        //                                                         .ThenInclude(x => x.CompetitionCategory)
        //                                                         .Include(x => x.Round));

        //        if (registrationRound == null)
        //        {
        //            throw new NotFoundException($"Registration round with ID '{request.RegistrationRoundId}' not found.");
        //        }

        //        var competitionCategoryId = registrationRound.Registration.CompetitionCategoryId;

        //        // 🔒 2️⃣ Lock vòng thi để tránh race condition
        //        bool isLocked = await _cacheService.LockAsync($"lock_round_{registrationRound.RoundId}", TimeSpan.FromSeconds(5));
        //        if (!isLocked)
        //        {
        //            throw new Exception("Another process is already finalizing this round. Please wait.");
        //        }

        //        // 3️⃣ Tạo mới ScoreDetail cho trọng tài hiện tại
        //        var newScore = new ScoreDetail
        //        {
        //            RefereeAccountId = request.RefereeAccountId,
        //            RegistrationRoundId = request.RegistrationRoundId,
        //            TotalPointMinus = request.IsPass ? 0 : 100,
        //            IsPublic = false,
        //            InitialScore = 100
        //        };

        //        await scoreRepository.InsertAsync(newScore);
        //        await _unitOfWork.CommitAsync();

        //        // 4️⃣ Lấy danh sách tất cả phiếu bầu của cá này trong vòng thi
        //        var allVotes = await scoreRepository.GetListAsync(
        //            predicate: s => s.RegistrationRoundId == request.RegistrationRoundId);

        //        int totalVotes = allVotes.Count;
        //        int totalPass = allVotes.Count(s => s.TotalPointMinus == 0); // Đếm số trọng tài chọn Pass

        //        // 5️⃣ Kiểm tra số lượng trọng tài đã chấm điểm
        //        int totalReferees = await refereeAssignmentRepository.CountAsync(
        //            predicate: r => r.RoundType == registrationRound.Round.RoundType
        //                            && r.CompetitionCategoryId == competitionCategoryId);

        //        if (totalReferees == 0)
        //        {
        //            throw new Exception($"No referees assigned for category {competitionCategoryId} in this round.");
        //        }

        //        bool isLastJudge = totalVotes >= totalReferees; // Nếu số phiếu chấm bằng số trọng tài → Trọng tài cuối cùng

        //        // 🔥 6️⃣ Chỉ xử lý RoundResult nếu là trọng tài cuối cùng
        //        if (isLastJudge)
        //        {
        //            // 7️⃣ Lấy danh sách RoundResult của vòng hiện tại
        //            var roundResults = await roundResultRepository.GetListAsync(
        //                predicate: r => r.RegistrationRounds.RoundId == registrationRound.RoundId,
        //                orderBy: query => query.OrderByDescending(r => r.TotalScore));

        //            // 8️⃣ Lấy NumberOfRegistrationToAdvance từ vòng hiện tại
        //            var roundInfo = await roundRepository.SingleOrDefaultAsync(
        //                predicate: r => r.Id == registrationRound.RoundId);

        //            if (roundInfo == null)
        //            {
        //                throw new Exception($"Round information not found for Round ID {registrationRound.RoundId}.");
        //            }

        //            int numberOfRegistrationsToAdvance = roundInfo.NumberOfRegistrationToAdvance ?? 0;

        //            // 9️⃣ Cập nhật Pass/Fail dựa trên số lượng cá cao điểm nhất
        //            var roundResultsList = roundResults.ToList();

        //            for (int i = 0; i < roundResultsList.Count; i++)
        //            {
        //                roundResultsList[i].Status = i < numberOfRegistrationsToAdvance ? "Pass" : "Fail";
        //                  roundResultRepository.UpdateAsync(roundResultsList[i]);
        //            }

        //            await _unitOfWork.CommitAsync();
        //        }

        //        // 🔥 10️⃣ Giải phóng LOCK
        //        await _cacheService.UnlockAsync($"lock_round_{registrationRound.RoundId}");

        //        // 🔥 11️⃣ Gửi cập nhật điểm số qua SignalR
        //       // await _scoreHub.Clients.All.SendAsync("ReceiveFinalRoundScores", request.RegistrationRoundId);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        await _cacheService.UnlockAsync($"lock_round_{registrationRound.RoundId}");
        //        throw new Exception($"Failed to process elimination score: {ex.Message}");
        //    }
        //}

    }
}