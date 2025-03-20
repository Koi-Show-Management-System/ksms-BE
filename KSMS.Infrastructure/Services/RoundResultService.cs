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
using KSMS.Domain.Dtos.Responses.FinalResult;
using KSMS.Domain.Dtos.Responses.KoiMedium;
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

        public async Task PublishRoundResult(Guid roundId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var round = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(
                    predicate: r => r.Id == roundId,
                    include: query => query
                        .Include(r => r.RegistrationRounds)
                            .ThenInclude(rr => rr.Registration)
                        .Include(r => r.RegistrationRounds)
                            .ThenInclude(rr => rr.RoundResults));
                if (round == null)
                {
                    throw new NotFoundException("Round not found.");
                }
                var isFinalRound = false;
                if (round.RoundType == RoundEnum.Final.ToString())
                {
                    var highestOrderInCategory = await _unitOfWork.GetRepository<Round>()
                        .GetListAsync(
                            predicate: r => r.CompetitionCategoriesId == round.CompetitionCategoriesId,
                            orderBy: q => q.OrderByDescending(r => r.RoundOrder)
                        );
            
                    isFinalRound = highestOrderInCategory.First().Id == round.Id;
                }
                var registrationRounds = round.RegistrationRounds.ToList();
                if (!registrationRounds.Any())
                {
                    throw new BadRequestException("No registration rounds found for this round.");
                }
                
                var registrationRoundWithoutResult = registrationRounds
                    .Where(rr => !rr.RoundResults.Any())
                    .Select(rr => rr.Registration.RegisterName).ToList();
                if (registrationRoundWithoutResult.Any())
                {
                    
                    throw new BadRequestException($"Registration(s) {string.Join(", ", registrationRoundWithoutResult)} do not have results.");
                }
                if (round.RoundType == RoundEnum.Preliminary.ToString())
                {
                    var totalParticipants = registrationRounds.Count;
                    var passCount = registrationRounds.Count(rr => rr.RoundResults.First().Status == "Pass");

                    foreach (var regisRound in registrationRounds)
                    {
                        var roundResult = regisRound.RoundResults.First();
                        roundResult.IsPublic = true;
                        _unitOfWork.GetRepository<RoundResult>().UpdateAsync(roundResult);

                        var registration = regisRound.Registration;
                        registration.Rank = roundResult.Status == "Pass" ? passCount : totalParticipants;
                        if (roundResult.Status == "Fail")
                        {
                            registration.Status = "eliminated";
                        }
                        _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
                    }
                }
                else
                {
                    var sortedResults = registrationRounds
                        .OrderByDescending(rr => rr.RoundResults.First().TotalScore)
                        .ToList();

                    var currentRank = 1;
                    var skipCount = 0;
                    decimal? previousScore = null;

                    for (int i = 0; i < sortedResults.Count; i++)
                    {
                        var regisRound = sortedResults[i];
                        var currentScore = regisRound.RoundResults.First().TotalScore;

                        if (previousScore != currentScore)
                        {
                            currentRank = i + 1;
                        }
                        else
                        {
                            skipCount++;
                        }

                        var roundResult = regisRound.RoundResults.First();
                        roundResult.IsPublic = true;
                        _unitOfWork.GetRepository<RoundResult>().UpdateAsync(roundResult);

                        var registration = regisRound.Registration;
                        registration.Rank = currentRank + skipCount;
                        if (roundResult.Status == "Fail")
                        {
                            registration.Status = "eliminated";
                        }else if (isFinalRound)
                        {
                            registration.Status = "completed";
                        }
                        
                        _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);

                        previousScore = currentScore;
                    }
                }
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<FinalResultResponse>> GetFinalResultByCategoryId(Guid categoryId)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate:
                x => x.Id == categoryId);
            if (category == null)
            {
                throw new NotFoundException("Category not found.");
            }
            var finalRound = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(
                predicate: r => r.CompetitionCategoriesId == categoryId
                                && r.RoundType == RoundEnum.Final.ToString(),
                include: query => query
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Registration)
                            .ThenInclude(r => r.KoiProfile)
                                .ThenInclude(rr => rr.Variety)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(rr => rr.Registration)
                            .ThenInclude(r => r.KoiMedia)
                    .Include(r => r.RegistrationRounds)
                        .ThenInclude(r => r.RoundResults),
                orderBy: q => q.OrderByDescending(r => r.RoundOrder));
            if (finalRound == null)
            {
                throw new NotFoundException("Final Round not found");
            }

            var awards = await _unitOfWork.GetRepository<Award>().GetListAsync(predicate:
                a => a.CompetitionCategoriesId == categoryId);
            var results = finalRound.RegistrationRounds
                .Where(rr => rr.RoundResults.Any() && rr.RoundResults.First().IsPublic.Value)
                .Select(rr => new FinalResultResponse
                {
                    RegistrationId = rr.RegistrationId,
                    RegistrationNumber = rr.Registration.RegistrationNumber,
                    RegisterName = rr.Registration.RegisterName,
                    KoiSize = rr.Registration.KoiSize,
                    Rank = rr.Registration.Rank ?? 0,
                    FinalScore = rr.RoundResults.First().TotalScore,
                    Status = rr.Registration.Status,
                    KoiName = rr.Registration.KoiProfile.Name,
                    Gender = rr.Registration.KoiProfile.Gender,
                    Bloodline = rr.Registration.KoiProfile.Bloodline,
                    Variety = rr.Registration.KoiProfile.Variety?.Name,

                    Media = rr.Registration.KoiMedia.Select(km => new GetKoiMediaResponse
                    {
                        Id = km.Id,
                        MediaUrl = km.MediaUrl,
                        MediaType = km.MediaType
                    }).ToList()
                }).OrderByDescending(r => r.FinalScore).ToList();
            foreach (var result in results)
            {
                var award = result.Rank switch
                {
                    1 => awards.FirstOrDefault(a => a.AwardType == "Giải nhất"),
                    2 => awards.FirstOrDefault(a => a.AwardType == "Giải nhì"),
                    3 => awards.FirstOrDefault(a => a.AwardType == "Giải ba"),
                    _ => null
                };
                if (award == null) continue;
                result.AwardType = award.AwardType;
                result.AwardName = award.Name;
                result.PrizeValue = award.PrizeValue;
            }
            return results;

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
