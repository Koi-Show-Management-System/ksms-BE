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
using KSMS.Infrastructure.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace KSMS.Infrastructure.Services
{
    public class RoundResultService : BaseService<RoundResultService>, IRoundResultService
    {
        private readonly IHubContext<ScoreHub> _scoreHub;
        private readonly ICacheService _cacheService;

        public RoundResultService(
            IUnitOfWork<KoiShowManagementSystemContext> unitOfWork,
            ILogger<RoundResultService> logger,
            IHttpContextAccessor httpContextAccessor,
            IHubContext<ScoreHub> scoreHub,
            ICacheService cacheService
        ) : base(unitOfWork, logger, httpContextAccessor)
        {
            _scoreHub = scoreHub;
            _cacheService = cacheService;
        }
        public async Task ProcessFinalScoresForRound(Guid roundId)
        {
            try
            {
                var scoreRepository = _unitOfWork.GetRepository<ScoreDetail>();
                var roundResultRepository = _unitOfWork.GetRepository<RoundResult>();
                var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
                var registrationRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
                var roundRepository = _unitOfWork.GetRepository<Round>();

                // 1️⃣ Kiểm tra vòng thi có tồn tại
                var round = await roundRepository.SingleOrDefaultAsync(predicate: r => r.Id == roundId);
                if (round == null)
                {
                    throw new NotFoundException($"❌ Round with ID '{roundId}' not found.");
                }

                // 2️⃣ Lấy danh sách `RegistrationRound` của vòng thi này
                var registrationRounds = await registrationRoundRepository.GetListAsync(
                    predicate: rr => rr.RoundId == roundId,
                    include: query => query.Include(rr => rr.Registration)
                                           .ThenInclude(r => r.CompetitionCategory));

                if (!registrationRounds.Any())
                {
                    throw new NotFoundException($"❌ No registration rounds found for round {roundId}.");
                }

                // 3️⃣ Nhóm cá theo `CompetitionCategoryId`
                var groupedByCategory = registrationRounds.GroupBy(rr => rr.Registration.CompetitionCategoryId);

                foreach (var categoryGroup in groupedByCategory)
                {
                    var competitionCategoryId = categoryGroup.Key;
                    var registrationsInCategory = categoryGroup.ToList();
                    int numberOfRegistrationsToAdvance = round.NumberOfRegistrationToAdvance ?? 0;

                    // 4️⃣ Kiểm tra số lượng trọng tài trong vòng này
                    int assignedReferees = await refereeAssignmentRepository.CountAsync(
                        predicate: r => r.RoundType == round.RoundType
                                        && r.CompetitionCategoryId == competitionCategoryId);

                    if (assignedReferees == 0)
                    {
                        throw new Exception($"❌ No referees assigned for category {competitionCategoryId} in this round.");
                    }

                    // 5️⃣ Tính tổng điểm của mỗi cá
                    var finalScores = new Dictionary<Guid, decimal>();

                    foreach (var registrationRound in registrationsInCategory)
                    {
                        // ⚠ Lấy danh sách điểm của cá trong vòng thi
                        var scores = await scoreRepository.GetListAsync(
                            predicate: s => s.RegistrationRoundId == registrationRound.Id);

                        int totalReferees = scores.Count;

                        if (totalReferees < assignedReferees)
                        {
                            throw new Exception($"⚠ Not all referees have scored RegistrationRound {registrationRound.Id}. ({totalReferees}/{assignedReferees})");
                        }

                        // Tổng điểm bị trừ
                        decimal totalPenalty = scores.Sum(s => s.TotalPointMinus);
                        decimal finalScore = 100 - (totalPenalty / assignedReferees);

                        finalScores.Add(registrationRound.Id, finalScore);
                    }

                    // 6️⃣ Xếp hạng theo điểm số
                    var sortedResults = finalScores.OrderByDescending(x => x.Value).ToList();
                    var roundResults = new List<RoundResult>();

                    for (int i = 0; i < sortedResults.Count; i++)
                    {
                        roundResults.Add(new RoundResult
                        {
                            RegistrationRoundsId = sortedResults[i].Key,
                            TotalScore = sortedResults[i].Value,
                            Status = i < numberOfRegistrationsToAdvance ? "Pass" : "Fail",
                            IsPublic = false
                        });
                    }

                    // 7️⃣ Cập nhật `RoundResult` nếu đã có, hoặc thêm mới nếu chưa có
                    foreach (var result in roundResults)
                    {
                        var existingResult = await roundResultRepository.SingleOrDefaultAsync(
                            predicate: r => r.RegistrationRoundsId == result.RegistrationRoundsId);

                        if (existingResult != null)
                        {
                            existingResult.TotalScore = result.TotalScore;
                            existingResult.Status = result.Status;
                            roundResultRepository.UpdateAsync(existingResult);
                        }
                        else
                        {
                            await roundResultRepository.InsertAsync(result);
                        }
                    }
                }

                // ✅ Commit UoW để đảm bảo dữ liệu được ghi vào DB
                await _unitOfWork.CommitAsync();
            }
            catch (NotFoundException ex)
            {
                _logger.LogError($"🚨 Not Found: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"🚨 Failed to process final scores for round {roundId}: {ex.Message}");
                throw new Exception($"Failed to process final scores for round {roundId}. Check logs for details.");
            }
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
        public async Task<Paginate<RegistrationGetByCategoryPagedResponse>> GetPagedRegistrationsByCategoryAndStatusAsync
            (Guid categoryId, RoundResultStatus? status, int page, int size)
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
