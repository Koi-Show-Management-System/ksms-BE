using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.RegistrationRound;
using KSMS.Domain.Dtos.Responses.RegistrationRound;
using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using KSMS.Application.Extensions;
using KSMS.Domain.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using KSMS.Domain.Dtos.Requests.Registration;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Utils;
using System.Linq;

namespace KSMS.Infrastructure.Services
{
    public class RegistrationRoundService : BaseService<RegistrationRoundService>, IRegistrationRoundService
    {
        private readonly ITankService _tankService;

        public RegistrationRoundService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork,
            ILogger<RegistrationRoundService> logger, IHttpContextAccessor httpContextAccessor,
            ITankService tankService) : base(unitOfWork, logger, httpContextAccessor)
        {
            _tankService = tankService;
        }

        public async Task<RegistrationRoundResponse> CreateRegistrationRoundAsync(
            CreateRegistrationRoundRequest request)
        {
            var registrationRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();

            // Kiểm tra nếu vòng đăng ký đã tồn tại
            var existingRegistrationRound = await registrationRoundRepository.SingleOrDefaultAsync(
                predicate: rr => rr.RegistrationId == request.RegistrationId && rr.RoundId == request.RoundId
            );

            if (existingRegistrationRound != null)
            {
                throw new BadRequestException("Vòng đăng ký này đã tồn tại cho cá này.");
            }

            // Chuyển đổi từ DTO sang Entity
            var registrationRound = request.Adapt<RegistrationRound>();

            // Lưu RegistrationRound vào cơ sở dữ liệu
            var createdRegistrationRound = await registrationRoundRepository.InsertAsync(registrationRound);
            await _unitOfWork.CommitAsync();

            return createdRegistrationRound.Adapt<RegistrationRoundResponse>();
        }

        public async Task UpdateFishesWithTanks(List<UpdateFishTankRequest> updateRequests)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
                var tankRepository = _unitOfWork.GetRepository<Tank>();

                // 1️⃣ Lấy danh sách RegistrationRoundId từ yêu cầu cập nhật
                var registrationRoundIds = updateRequests.Select(r => r.RegistrationRoundId).ToList();
                var existingRegistrations = await regisRoundRepository.GetListAsync(
                    predicate: rr => registrationRoundIds.Contains(rr.Id));
                var round = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(predicate: x => x.Id == existingRegistrations.First().RoundId);
                if (!existingRegistrations.Any())
                {
                    throw new Exception("Không tìm thấy đăng ký hợp lệ cho các ID đã cung cấp.");
                }

                // 2️⃣ Lấy danh sách TankId từ yêu cầu cập nhật
                var tankIds = updateRequests.Select(r => r.TankId).Distinct().ToList();
                var availableTanks = await tankRepository.GetListAsync(
                    predicate: t => tankIds.Contains(t.Id) && t.Status == TankStatus.Available.ToString().ToLower()
                );

                if (availableTanks.Count != tankIds.Count)
                {
                    throw new Exception("Một hoặc nhiều hồ được chọn không khả dụng hoặc không tồn tại.");
                }

                // 3️⃣ Kiểm tra sức chứa của hồ trước khi cập nhật
                var tankFishCounts = await Task.WhenAll(availableTanks.Select(async tank =>
                    new { Tank = tank, FishCount = await _tankService.GetCurrentFishCount(tank.Id, round.Id) }));

                foreach (var tank in tankFishCounts)
                {
                    // 🔥 *Tính số cá mới sẽ vào hồ*
                    int newFishCount = updateRequests
                        .Where(r => r.TankId == tank.Tank.Id)
                        .Count(r => existingRegistrations.FirstOrDefault(regis => regis.Id == r.RegistrationRoundId)
                            ?.TankId != tank.Tank.Id);

                    // 🛠 **Nếu cá đã ở trong hồ, không tăng FishCount**
                    if (tank.FishCount + newFishCount > tank.Tank.Capacity)
                    {
                        throw new Exception(
                            $"Hồ {tank.Tank.Id} không đủ chỗ trống. Sức chứa: {tank.Tank.Capacity}, Đã có: {tank.FishCount}, Thêm mới: {newFishCount}");
                    }
                }

                // 4️⃣ Cập nhật TankId cho RegistrationRound
                foreach (var update in updateRequests)
                {
                    var regis = existingRegistrations.FirstOrDefault(r => r.Id == update.RegistrationRoundId);
                    if (regis != null)
                    {
                        regis.TankId = update.TankId; // ✅ Cập nhật hồ cho cá
                        regisRoundRepository.UpdateAsync(regis);
                    }
                }

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AssignMultipleFishesToTankAndRound(Guid roundId, List<Guid> registrationIds)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
                var registrationRepository = _unitOfWork.GetRepository<Registration>();
                var roundRepository = _unitOfWork.GetRepository<Round>();

                // 1️⃣ Kiểm tra danh sách cá hợp lệ
                if (registrationIds == null || !registrationIds.Any())
                {
                    throw new BadRequestException("Danh sách cá không được để trống.");
                }

                // 2️⃣ Kiểm tra RoundId có tồn tại không
                var roundExists = (await roundRepository.GetListAsync(predicate: r => r.Id == roundId)).Any();
                if (!roundExists)
                {
                    throw new NotFoundException($"Không tìm thấy vòng thi {roundId}. Vui lòng tạo vòng thi trước.");
                }
                var existingInRound = await regisRoundRepository.GetListAsync(
                    predicate: rr => rr.RoundId == roundId && registrationIds.Contains(rr.RegistrationId));
        
                if (existingInRound.Any())
                {
                    var duplicateIds = existingInRound.Select(r => r.RegistrationId).ToList();
                    throw new BadRequestException($"Tất cả cá đã được phân vào vòng tiếp theo");
                }
                // 3️⃣ Lấy danh sách đơn đăng ký của cá
                var registrations = await registrationRepository.GetListAsync(
                    predicate: r => registrationIds.Contains(r.Id));

                // 4️⃣ Kiểm tra cùng hạng mục
                var categoryId = registrations.First().CompetitionCategoryId;
                if (registrations.Any(r => r.CompetitionCategoryId != categoryId))
                {
                    throw new BadRequestException("Tất cả các cá phải thuộc cùng một hạng mục.");
                }

                // 5️⃣ Lấy danh sách cá đã thi đấu trong vòng trước
                var existingRegistrations = await regisRoundRepository.GetListAsync(
                    predicate: rr => registrationIds.Contains(rr.RegistrationId));

                List<RegistrationRound> newRegisRounds = new();

                // 6️⃣ Tạo bản ghi mới cho vòng mới
                foreach (var registration in registrations)
                {
                    newRegisRounds.Add(new RegistrationRound
                    {
                        Id = Guid.NewGuid(), // Luôn tạo mới
                        RegistrationId = registration.Id,
                        RoundId = roundId,
                        CheckInTime = VietNamTimeUtil.GetVietnamTime(),
                        Status = "unpublic",
                        CreatedAt = VietNamTimeUtil.GetVietnamTime()
                    });
                    
                    registration.Status = "competition";
                    registrationRepository.UpdateAsync(registration);
                }

                // 🔥 7️⃣ Chèn bản ghi mới vào bảng
                await regisRoundRepository.InsertRangeAsync(newRegisRounds);

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Paginate<GetPageRegistrationRoundResponse>> GetPageRegistrationRound(Guid roundId, int page,
            int size)
        {
            var round = await _unitOfWork.GetRepository<Round>()
                .SingleOrDefaultAsync(predicate: x => x.Id == roundId);
            if (round == null)
            {
                throw new NotFoundException("Không tìm thấy vòng thi");
            }

            var role = GetRoleFromJwt();
            // Tạo predicate cơ bản
            Expression<Func<RegistrationRound, bool>> predicate = x => x.RoundId == roundId;

            // Nếu vai trò là REFEREE (trọng tài), chỉ lấy những RegistrationRound mà trọng tài đó chấm điểm
            if (role.ToUpper() == "REFEREE")
            {
                predicate = predicate.AndAlso(x => x.ScoreDetails.Any(sd => sd.RefereeAccountId == GetIdFromJwt()));
            }

            if (role.ToUpper() == "MEMBER" || role.ToUpper() == "GUEST")
            {
                predicate = predicate.AndAlso(x => x.Status == "public");
            }
            var criteriaCompetitionCategories = await _unitOfWork.GetRepository<CriteriaCompetitionCategory>()
                .GetListAsync(predicate: c => c.CompetitionCategoryId == round.CompetitionCategoriesId &&
                                              c.RoundType == round.RoundType);
            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>().GetPagingListAsync(
                predicate: predicate,
                include: query => query.AsSplitQuery()
                    .Include(x => x.RoundResults)
                    .Include(x => x.ScoreDetails)
                    .ThenInclude(sd => sd.ScoreDetailErrors)
                    .ThenInclude(err => err.ErrorType)
                    .Include(x => x.Tank)
                    .Include(x => x.Registration)
                    .ThenInclude(x => x.CompetitionCategory)
                    .Include(x => x.Registration)
                    .ThenInclude(x => x.KoiProfile)
                    .ThenInclude(x => x.Variety)
                    .Include(x => x.Registration)
                    .ThenInclude(x => x.KoiShow)
                    .Include(x => x.Registration)
                    .ThenInclude(x => x.KoiMedia)
                    .Include(x => x.RoundResults),
                orderBy: q =>
                {
                    if ((role.ToUpper() == "MEMBER" || role.ToUpper() == "GUEST") 
                        && !q.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true)))
                    {
                        return q.OrderBy(x => x.CreatedAt);
                    }

                    if (q.All(x => x.RoundResults.Any()))
                    {
                        return q.OrderByDescending(x => x.RoundResults.First().TotalScore);
                    }
                    return q.OrderBy(x => x.CreatedAt);
                },
                page: page,
                size: size);
            //var response = registrationRounds.Adapt<Paginate<GetPageRegistrationRoundResponse>>();
            var items = registrationRounds.Items.ToList();
            if (items.All(x => x.RoundResults.Any()) && 
                ((role.ToUpper() != "MEMBER" && role.ToUpper() != "GUEST") || 
                 items.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true))))
            {
                items = items
                    .OrderByDescending(x => x.RoundResults.First().TotalScore)
                    .ThenBy(x => GetHighestWeightCriteriaDeduction(
                        x.ScoreDetails.ToList(), 
                        criteriaCompetitionCategories.ToList()))
                    .ToList();
            }
            
            var response = new Paginate<GetPageRegistrationRoundResponse>
            {
                Items = items.Adapt<List<GetPageRegistrationRoundResponse>>(),
                Total = registrationRounds.Total,
                Page = registrationRounds.Page,
                Size = registrationRounds.Size,
                TotalPages = registrationRounds.TotalPages
            };
            bool hasRoundResults = items.All(x => x.RoundResults.Any());
            
            // Xử lý đặc biệt cho vòng Preliminary
            if (round.RoundType == "Preliminary" && hasRoundResults)
            {
                // Đếm số lượng đăng ký pass và tổng số đăng ký
                int totalPassed = items.Count(x => x.RoundResults.Any() && 
                                                  x.RoundResults.FirstOrDefault()?.Status?.ToLower() == "pass");
                int totalRegistrations = items.Count;
                
                // Kiểm tra xem người dùng có quyền xem kết quả chi tiết không
                bool canViewDetailedResults;
                if (role.ToUpper() == "MEMBER" || role.ToUpper() == "GUEST")
                {
                    // Nếu là MEMBER/GUEST, chỉ có quyền xem kết quả chi tiết nếu kết quả được public
                    canViewDetailedResults = items.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true));
                }
                else
                {
                    // Nếu là ADMIN/ORGANIZER/REFEREE, luôn có quyền xem kết quả chi tiết
                    canViewDetailedResults = true;
                }
                
                // Gán rank dựa vào kết quả pass/fail
                foreach (var item in response.Items)
                {
                    var originalItem = items.FirstOrDefault(x => x.Id == item.Id);
                    
                    // Nếu người dùng không có quyền xem kết quả chi tiết, hoặc kết quả chưa được công khai
                    if (!canViewDetailedResults)
                    {
                        // Tất cả cá có cùng rank bằng tổng số đăng ký
                        item.Rank = totalRegistrations;
                    }
                    // Nếu có quyền xem và có kết quả
                    else if (originalItem != null && originalItem.RoundResults.Any())
                    {
                        // Kiểm tra status để xác định pass
                        bool isPassed = originalItem.RoundResults.FirstOrDefault()?.Status?.ToLower() == "pass";
                        
                        // Nếu pass thì rank = số người pass, nếu fail thì rank = tổng số đăng ký
                        item.Rank = isPassed ? totalPassed : totalRegistrations;
                    }
                    else
                    {
                        item.Rank = totalRegistrations;
                    }
                }
            }
            // Xử lý các trường hợp không phải vòng Preliminary
            else
            {
                // Nếu chưa có round result, gán rank cho tất cả item không phụ thuộc quyền
                if (!hasRoundResults)
                {
                    int totalRegistrations = items.Count;
                    foreach (var item in response.Items)
                    {
                        item.Rank = totalRegistrations;
                    }
                }
                // Nếu có round result, chỉ gán rank dựa vào quyền
                else if (((role.ToUpper() != "MEMBER" && role.ToUpper() != "GUEST") ||
                          items.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true))))
                {
                    // Gán rank theo thứ tự của items khi có round result
                    for (int i = 0; i < response.Items.Count; i++)
                    {
                        response.Items[i].Rank = i + 1;
                    }
                }
            }
            
            foreach (var registrationRound in response.Items)
            {
                var registrationRoundEntity =
                    registrationRounds.Items.FirstOrDefault(x => x.Id == registrationRound.Id);
                registrationRound.TankName = registrationRoundEntity?.Tank?.Name;
                
                if (registrationRoundEntity != null && (role.ToUpper() == "MEMBER" || role.ToUpper() == "GUEST") &&
                    registrationRoundEntity.RoundResults.All(rr => rr.IsPublic != true))
                {
                    registrationRound.RoundResults = [];
                }
            }

            return response;
        }

        public async Task<CheckQrRegistrationRoundResponse> GetRegistrationRoundByIdAndRoundAsync(Guid registrationId,
            Guid roundId)
        {
            var registrationRound = await _unitOfWork.GetRepository<RegistrationRound>().SingleOrDefaultAsync(
                predicate: r => r.RegistrationId == registrationId && r.RoundId == roundId,
                include: query => query
                    .Include(r => r.Registration)
                    .ThenInclude(r => r.KoiProfile)
                    .ThenInclude(r => r.Variety)
                    .Include(r => r.Registration)
                    .ThenInclude(r => r.KoiMedia));
            if (registrationRound == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin đăng ký vòng thi.");
            }

            return registrationRound.Adapt<CheckQrRegistrationRoundResponse>();
        }

        public async Task PublishRound(Guid roundId)
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
                        .ThenInclude(rr => rr.RoundResults));
                if (round == null)
                {
                    throw new NotFoundException("Không tìm thấy vòng thi.");
                }

                var registrationRounds = round.RegistrationRounds.ToList();
                if (!registrationRounds.Any())
                {
                    throw new BadRequestException("Không tìm thấy đăng ký nào trong vòng này.");
                }

                if (round.CompetitionCategories.HasTank)
                {
                    var registrationRoundsWithoutTank = registrationRounds
                        .Where(rr => !rr.TankId.HasValue).ToList();
                    if (registrationRoundsWithoutTank.Any())
                    {
                        throw new BadRequestException(
                            "Tất cả các cá phải được phân vào hồ trước khi công bố vòng thi.");
                    }
                }
                var totalParticipants = registrationRounds.Count;
                foreach (var regisRound in registrationRounds)
                {
                    regisRound.Status = "public";
                    _unitOfWork.GetRepository<RegistrationRound>().UpdateAsync(regisRound);
                    var registration = regisRound.Registration;
                    registration.Rank = totalParticipants;
                    _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
                }

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
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
    }
}