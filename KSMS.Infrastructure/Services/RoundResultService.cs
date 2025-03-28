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
                var awardRepository = _unitOfWork.GetRepository<Award>();

                // 1️⃣ Kiểm tra vòng thi có tồn tại
                var round = await roundRepository.SingleOrDefaultAsync(predicate: r => r.Id == roundId,
                    include: query => query.Include(r => r.CompetitionCategories));
                if (round == null)
                {
                    throw new NotFoundException($"Không tìm thấy vòng thi có ID '{roundId}'.");
                }
                var isFinalRound = false;
                if (round.RoundType == RoundEnum.Final.ToString())
                {
                    var highestOrderInCategory = await roundRepository.GetListAsync(
                        predicate: r => r.CompetitionCategoriesId == round.CompetitionCategoriesId &&
                                        r.RoundType == RoundEnum.Final.ToString(),
                        orderBy: q => q.OrderByDescending(r => r.RoundOrder)
                    );
                    isFinalRound = highestOrderInCategory.First().Id == round.Id;
                }
                // 2️⃣ Lấy danh sách RegistrationRound của vòng thi này
                var registrationRounds = await registrationRoundRepository.GetListAsync(
                    predicate: rr => rr.RoundId == roundId,
                    include: query => query.Include(rr => rr.Registration)
                                           .ThenInclude(r => r.CompetitionCategory));

                if (!registrationRounds.Any())
                {
                    throw new NotFoundException($"Không tìm thấy đăng ký nào trong vòng thi {roundId}.");
                }

                // 3️⃣ Nhóm cá theo CompetitionCategoryId
                var groupedByCategory = registrationRounds.GroupBy(rr => rr.Registration.CompetitionCategoryId);

                foreach (var categoryGroup in groupedByCategory)
                {
                    var competitionCategoryId = categoryGroup.Key;
                    var registrationsInCategory = categoryGroup.ToList();
                    int numberOfRegistrationsToAdvance = round.NumberOfRegistrationToAdvance ?? 0;

                    int totalAwards = 0;
                    if (isFinalRound)
                    {
                        var awards = await awardRepository.GetListAsync(
                            predicate: a => a.CompetitionCategoriesId == competitionCategoryId);
                        totalAwards = awards.Count;
                        numberOfRegistrationsToAdvance = totalAwards;
                    }

                    // 4️⃣ Kiểm tra số lượng trọng tài trong vòng này
                    int assignedReferees = await refereeAssignmentRepository.CountAsync(
                        predicate: r => r.RoundType == round.RoundType
                                        && r.CompetitionCategoryId == competitionCategoryId);

                    if (assignedReferees == 0)
                    {
                        throw new Exception($"Không có trọng tài nào được phân công cho hạng mục {competitionCategoryId} trong vòng này.");
                    }
                    
                    // 5️⃣ Tính tổng điểm của mỗi cá
                    var scoreDataByRegistration = new Dictionary<Guid, (decimal FinalScore, List<ScoreDetail> ScoreDetails)>();
                    //var finalScores = new Dictionary<Guid, decimal>();

                    foreach (var registrationRound in registrationsInCategory)
                    {
                        // ⚠ Lấy danh sách điểm của cá trong vòng thi
                        var scores = await scoreRepository.GetListAsync(
                            predicate: s => s.RegistrationRoundId == registrationRound.Id,
                            include: query => query
                                .Include(s => s.ScoreDetailErrors)
                                    .ThenInclude(e => e.ErrorType));

                        int totalReferees = scores.Count;

                        if (totalReferees < assignedReferees)
                        {
                            throw new Exception($"Chưa đủ trọng tài chấm điểm cho đăng ký {registrationRound.Id}. ({totalReferees}/{assignedReferees})");
                        }

                        // Tổng điểm bị trừ
                        decimal totalPenalty = scores.Sum(s => s.TotalPointMinus);
                        decimal finalScore = 100 - (totalPenalty / assignedReferees);

                        scoreDataByRegistration.Add(registrationRound.Id, (finalScore, scores.ToList()));
                    }
                    var criteriaCompetitionCategories = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                        .GetListAsync(predicate: c => c.CompetitionCategoryId == competitionCategoryId &&
                                                      c.RoundType == round.RoundType);
                    // 6️⃣ Xếp hạng theo điểm số
                    var sortedResults = scoreDataByRegistration
                        .OrderByDescending(x => x.Value.FinalScore)
                        .ThenBy(x => GetHighestWeightCriteriaDeduction(x.Value.ScoreDetails, criteriaCompetitionCategories.ToList()))
                        .Select(x => new {RegistrationRoundId = x.Key, Score = x.Value.FinalScore})
                        .ToList();
                    var roundResults = new List<RoundResult>();

                    for (int i = 0; i < sortedResults.Count; i++)
                    {
                        int rank = i + 1;
                        string status;
                        if (isFinalRound)
                        {
                            status = rank <= totalAwards ? "Pass" : "Fail";
                        }
                        else
                        {
                            status = rank <= numberOfRegistrationsToAdvance ? "Pass" : "Fail";
                        }
                        roundResults.Add(new RoundResult
                        {
                            RegistrationRoundsId = sortedResults[i].RegistrationRoundId,
                            TotalScore = sortedResults[i].Score,
                            Status = status,//i < numberOfRegistrationsToAdvance ? "Pass" : "Fail",
                            IsPublic = false
                        });
                    }

                    // 7️⃣ Cập nhật RoundResult nếu đã có, hoặc thêm mới nếu chưa có
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
                _logger.LogError($"🚨 Không tìm thấy: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"🚨 Không thể xử lý điểm số cuối cùng cho vòng {roundId}: {ex.Message}");
                throw new Exception($"Không thể xử lý điểm số cuối cùng cho vòng {roundId}. Kiểm tra logs để biết thêm chi tiết.");
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
                throw new NotFoundException("Không tìm thấy kết quả vòng thi cho hạng mục này");
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
                        .Include(r => r.CompetitionCategories)
                        .Include(r => r.RegistrationRounds)
                            .ThenInclude(rr => rr.Registration)
                        .Include(r => r.RegistrationRounds)
                            .ThenInclude(rr => rr.RoundResults)
                        .Include(r => r.RegistrationRounds)
                            .ThenInclude(rr => rr.ScoreDetails)
                                .ThenInclude(sd => sd.ScoreDetailErrors)
                                    .ThenInclude(sde => sde.ErrorType));
                if (round == null)
                {
                    throw new NotFoundException("Không tìm thấy vòng thi.");
                }
                var isFinalRound = false;
                if (round.RoundType == RoundEnum.Final.ToString())
                {
                    var highestOrderInCategory = await _unitOfWork.GetRepository<Round>()
                        .GetListAsync(
                            predicate: r => r.CompetitionCategoriesId == round.CompetitionCategoriesId &&
                                            r.RoundType == RoundEnum.Final.ToString(),
                            orderBy: q => q.OrderByDescending(r => r.RoundOrder)
                        );
            
                    isFinalRound = highestOrderInCategory.First().Id == round.Id;
                }
                var registrationRounds = round.RegistrationRounds.ToList();
                if (!registrationRounds.Any())
                {
                    throw new BadRequestException("Không tìm thấy đăng ký nào trong vòng này.");
                }
                
                var registrationRoundWithoutResult = registrationRounds
                    .Where(rr => !rr.RoundResults.Any())
                    .Select(rr => rr.Registration.RegisterName).ToList();
                if (registrationRoundWithoutResult.Any())
                {
                    throw new BadRequestException($"Các đăng ký sau chưa có kết quả: {string.Join(", ", registrationRoundWithoutResult)}");
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
                    List<Award> awards = new List<Award>();
                    int totalAwards = 0;
                    if (isFinalRound)
                    {
                        awards = (await _unitOfWork.GetRepository<Award>().GetListAsync(
                            predicate: a => a.CompetitionCategoriesId == round.CompetitionCategoriesId)).ToList();
                        totalAwards = awards.Count;
                        
                    }
                    var criteriaCompetitionCategories = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                        .GetListAsync(predicate: c => c.CompetitionCategoryId == round.CompetitionCategoriesId &&
                                                      c.RoundType == round.RoundType);
                    var sortedResults = registrationRounds
                        //.OrderByDescending(rr => rr.RoundResults.First().TotalScore)
                        .Select(rr => new
                        {
                            RegistrationRound = rr,
                            TotalScore = rr.RoundResults.First().TotalScore,
                            ScoreDetails = rr.ScoreDetails.ToList()
                        })
                        .OrderByDescending(x => x.TotalScore)
                        .ThenBy(x => GetHighestWeightCriteriaDeduction(x.ScoreDetails, criteriaCompetitionCategories.ToList()))
                        .Select(x => x.RegistrationRound)
                        .ToList();

                    var currentRank = 0;

                    for (int i = 0; i < sortedResults.Count; i++)
                    {
                        var regisRound = sortedResults[i];
                        currentRank = i + 1;

                        var roundResult = regisRound.RoundResults.First();
                        roundResult.IsPublic = true;
                        _unitOfWork.GetRepository<RoundResult>().UpdateAsync(roundResult);

                        var registration = regisRound.Registration;
                        registration.Rank = currentRank;
                        if (roundResult.Status == "Fail")
                        {
                            registration.Status = "eliminated";
                        }
                        else if (isFinalRound)
                        {
                            if (currentRank <= totalAwards)
                            {   
                                registration.Status = "prizewinner";
                            }
                            else
                            {
                                registration.Status = "eliminated";
                            }
                            
                        }
                        
                        _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
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
                throw new NotFoundException("Không tìm thấy hạng mục.");
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
                throw new NotFoundException("Không tìm thấy vòng chung kết");
            }

            var awards = await _unitOfWork.GetRepository<Award>().GetListAsync(predicate:
                a => a.CompetitionCategoriesId == categoryId);
            var results = finalRound.RegistrationRounds
                .Where(rr => rr.RoundResults.Any()
                             && rr.RoundResults.First().IsPublic.Value
                             && rr.Registration.Status == "prizewinner")
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
                    1 => awards.FirstOrDefault(a => a.AwardType == "first"),
                    2 => awards.FirstOrDefault(a => a.AwardType == "second"),
                    3 => awards.FirstOrDefault(a => a.AwardType == "third"),
                    4 => awards.FirstOrDefault(a => a.AwardType == "honorable"),
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

        private decimal GetHighestWeightCriteriaDeduction(List<ScoreDetail> scoreDetails,
            List<CriteriaCompetitionCategory> criteriaCompetitionCategories)
        {
            var highestWeightCriteria = criteriaCompetitionCategories
                .OrderByDescending(c => c.Weight)
                .FirstOrDefault();
            if (highestWeightCriteria == null)
            {
                return 0;
            }
            decimal totalDeduction = 0;
            foreach (var scoreDetail in scoreDetails)
            {
                var errorsForCriteria = scoreDetail.ScoreDetailErrors
                    .Where(err => err.ErrorType.CriteriaId == highestWeightCriteria.CriteriaId)
                    .Sum(err => err.PointMinus);
                totalDeduction += errorsForCriteria;
            }

            return totalDeduction;
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