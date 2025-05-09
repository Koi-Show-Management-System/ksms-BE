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
                            $"Hồ {tank.Tank.Name} không đủ chỗ trống. Sức chứa: {tank.Tank.Capacity}, Đã có: {tank.FishCount}, Thêm mới: {newFishCount}");
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

        public async Task AssignMultipleFishesToTankAndRound(Guid? currentRoundId, Guid roundId, List<Guid> registrationIds)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
                var registrationRepository = _unitOfWork.GetRepository<Registration>();
                var roundRepository = _unitOfWork.GetRepository<Round>();
                var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();

                // 1️⃣ Kiểm tra danh sách cá hợp lệ
                if (registrationIds == null || !registrationIds.Any())
                {
                    throw new BadRequestException("Danh sách cá không được để trống.");
                }

                // 2️⃣ Kiểm tra RoundId có tồn tại không
                var round = await roundRepository.SingleOrDefaultAsync(
                    predicate: r => r.Id == roundId);
                
                if (round == null)
                {
                    throw new NotFoundException($"Không tìm thấy vòng thi {roundId}. Vui lòng tạo vòng thi trước.");
                }

                // Kiểm tra xem vòng đấu đã được công khai chưa
                var publishedRegistrationsCount = await regisRoundRepository.CountAsync(
                    predicate: rr => rr.RoundId == roundId && rr.Status == "public");
                if (publishedRegistrationsCount > 0)
                {
                    throw new BadRequestException("Không thể thêm cá vào vòng thi đã được công khai.");
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
                
                // Kiểm tra trạng thái của hạng mục - không cho phép thêm cá vào vòng thi của hạng mục đã bị hủy
                var category = await categoryRepository.SingleOrDefaultAsync(
                    predicate: c => c.Id == categoryId,
                    include: query => query.Include(c => c.KoiShow));
                
                if (category == null)
                {
                    throw new NotFoundException("Không tìm thấy hạng mục thi đấu.");
                }
                
                if (category.Status?.ToLower() == CategoryStatus.Cancelled.ToString().ToLower())
                {
                    throw new BadRequestException("Hạng mục đã bị hủy. Bạn không thể đưa cá vào vòng thi của hạng mục đã bị hủy.");
                }
                
                // Kiểm tra số lượng đăng ký check-in đối với vòng Preliminary
                if (round.RoundType?.ToLower() == "preliminary")
                {
                    // Đếm số lượng đơn đăng ký đã check-in cho hạng mục này
                    var checkedInCount = await registrationRepository.CountAsync(
                        predicate: r => r.CompetitionCategoryId == categoryId && 
                                       r.Status == RegistrationStatus.CheckIn.ToString().ToLower());
                    
                    // Kiểm tra so với MinEntries trong category
                    if (category.MinEntries.HasValue && checkedInCount < category.MinEntries.Value)
                    {
                        throw new BadRequestException(
                            $"Hạng mục chưa đủ người check-in (hiện tại: {checkedInCount}/{category.MinEntries.Value}). " +
                            $"Cần chờ đủ số lượng hoặc nếu không đủ bạn có thể hủy hạng mục.");
                    }
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
                if (currentRoundId != null)
                {
                    var currentRound = await _unitOfWork.GetRepository<Round>().SingleOrDefaultAsync(
                        predicate: r => r.Id == currentRoundId);
                    if (currentRound == null)
                    {
                        throw new NotFoundException("Không tìm thấy vòng thi hiện tại.");
                    }
                    currentRound.Status = "completed";
                    roundRepository.UpdateAsync(currentRound);
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
            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>().GetPagingListAsync(
                predicate: predicate,
                include: query => query
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
                    .Include(x => x.Registration)
                    .ThenInclude(x => x.Account)
                    .Include(x => x.RoundResults),
                orderBy: q =>
                {
                    // Nếu người dùng có quyền xem (không phải MEMBER/GUEST) và đã có kết quả -> Sắp xếp theo Rank
                    if (role.ToUpper() != "MEMBER" && role.ToUpper() != "GUEST" && 
                        q.Any(x => x.RoundResults.Any()) && q.Any(x => x.Rank.HasValue))
                    {
                        return q.OrderBy(x => x.Rank);
                    }
                    // Nếu kết quả đã công bố công khai -> Sắp xếp theo Rank cho tất cả người dùng

                    if (q.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true)) && q.Any(x => x.Rank.HasValue))
                    {
                        return q.OrderBy(x => x.Rank);
                    }
                    // Nếu có kết quả nhưng chưa công bố và người dùng có quyền xem -> Sắp xếp theo điểm
                    // (Trường hợp này xử lý khi chưa có Rank hoặc mục đích sắp xếp khác)
                    if (q.Any(x => x.RoundResults.Any()) &&
                        (role.ToUpper() != "MEMBER" && role.ToUpper() != "GUEST"))
                    {
                        return q.OrderByDescending(x => x.RoundResults.First().TotalScore);
                    }

                    // Mặc định sắp xếp theo thời gian tạo
                    return q.OrderBy(x => x.Registration.RegistrationNumber);
                },
                page: page,
                size: size);
            var items = registrationRounds.Items.ToList();

            // Sử dụng Adapt để chuyển đổi toàn bộ đối tượng Paginate
            var response = registrationRounds.Adapt<Paginate<GetPageRegistrationRoundResponse>>();

            // Tính toán ban đầu cho tất cả các items
            var hasRoundResults = items.All(x => x.RoundResults.Any());
            var canViewDetailedResults = (role.ToUpper() != "MEMBER" && role.ToUpper() != "GUEST") || 
                                          items.Any(x => x.RoundResults.Any(rr => rr.IsPublic == true));
            
            // Lặp qua từng item để cập nhật Rank và các thông tin khác
            foreach (var item in response.Items)
            {
                var originalItem = items.FirstOrDefault(x => x.Id == item.Id);
                
                if (originalItem == null) continue;
                
                // Ưu tiên sử dụng Rank từ database
                if (originalItem.Rank.HasValue && 
                    (canViewDetailedResults || originalItem.RoundResults.Any(rr => rr.IsPublic == true)))
                {
                    // Sử dụng Rank đã lưu trong database
                    item.Rank = originalItem.Rank.Value;
                }
                // Nếu không có Rank trong database hoặc người dùng không có quyền xem
                else 
                {
                    // Sử dụng giá trị mặc định là tổng số đăng ký trong vòng
                    var totalRegistrations = await _unitOfWork.GetRepository<RegistrationRound>().CountAsync(
                        predicate: x => x.RoundId == roundId);
                    item.Rank = totalRegistrations;
                }
                
                // Xử lý hiển thị RoundResults
                if ((role.ToUpper() == "MEMBER" || role.ToUpper() == "GUEST") &&
                    hasRoundResults && originalItem.RoundResults.All(rr => rr.IsPublic != true))
                {
                    item.RoundResults = [];
                }
                
                // Gán TankName
                item.TankName = originalItem.Tank?.Name;
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
                    .ThenInclude(r => r.KoiMedia)
                    .Include(r => r.Registration)
                    .ThenInclude(r => r.KoiShow));
            if (registrationRound == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin đăng ký vòng thi.");
            }

            // Kiểm tra trạng thái của triển lãm
            if (registrationRound.Registration.KoiShow.Status.ToLower() != Domain.Enums.ShowStatus.InProgress.ToString().ToLower())
            {
                throw new BadRequestException("Triển lãm không trong giai đoạn đang diễn ra. Không thể xem thông tin vòng thi.");
            }

            // Kiểm tra xem vòng thi đã được công khai chưa
            if (registrationRound.Status.ToLower() != "public")
            {
                throw new BadRequestException("Vòng thi chưa được công khai. Xin hãy đợi vòng thi công khai sau đó thử quét lại.");
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

                // Kiểm tra xem có đơn đăng ký nào ở trạng thái check-in thuộc hạng mục này
                // mà chưa được thêm vào vòng thi hiện tại không
                if (round.RoundType?.ToLower() == "preliminary")
                {
                    var categoryId = round.CompetitionCategoriesId;
                    
                    // Lấy danh sách các đơn đăng ký đã check-in của hạng mục này
                    var checkedInRegistrations = await _unitOfWork.GetRepository<Registration>().GetListAsync(
                        predicate: r => r.CompetitionCategoryId == categoryId && 
                                      r.Status == RegistrationStatus.CheckIn.ToString().ToLower());
                    
                    // Lấy danh sách các đơn đăng ký đã được thêm vào vòng thi này
                    var registrationIdsInRound = registrationRounds
                        .Select(rr => rr.RegistrationId)
                        .ToList();
                    
                    // Tìm những đơn đăng ký đã check-in nhưng chưa được thêm vào vòng thi
                    var notInRound = checkedInRegistrations
                        .Where(r => !registrationIdsInRound.Contains(r.Id))
                        .ToList();
                    
                    if (notInRound.Any())
                    {
                        var missingCount = notInRound.Count;
                        throw new BadRequestException(
                            $"Còn {missingCount} đơn đăng ký đã check-in của hạng mục này chưa được thêm vào vòng thi. " +
                            "Hãy kiểm tra kỹ lại trước khi công khai vòng.");
                    }
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
                    regisRound.Rank = totalParticipants;
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

        public async Task AssignRegistrationsToPreliminaryRound(Guid categoryId, List<Guid> registrationIds)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var regisRoundRepository = _unitOfWork.GetRepository<RegistrationRound>();
                var registrationRepository = _unitOfWork.GetRepository<Registration>();
                var roundRepository = _unitOfWork.GetRepository<Round>();
                var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();

                // 1️⃣ Kiểm tra hạng mục có tồn tại không
                var category = await categoryRepository.SingleOrDefaultAsync(
                    predicate: c => c.Id == categoryId,
                    include: query => query.Include(c => c.KoiShow));
                
                if (category == null)
                {
                    throw new NotFoundException("Không tìm thấy hạng mục thi đấu.");
                }
                
                if (category.Status?.ToLower() == CategoryStatus.Cancelled.ToString().ToLower())
                {
                    throw new BadRequestException("Hạng mục đã bị hủy. Bạn không thể đưa cá vào vòng thi của hạng mục đã bị hủy.");
                }

                // 2️⃣ Tìm vòng Preliminary của hạng mục
                var preliminaryRound = await roundRepository.SingleOrDefaultAsync(
                    predicate: r => r.CompetitionCategoriesId == categoryId && 
                                    r.RoundType == RoundEnum.Preliminary.ToString());
                
                if (preliminaryRound == null)
                {
                    throw new NotFoundException($"Không tìm thấy vòng sơ khảo cho hạng mục này. Vui lòng tạo vòng thi trước.");
                }

                // 3️⃣ Kiểm tra xem vòng đấu đã được công khai chưa
                var publishedRegistrationsCount = await regisRoundRepository.CountAsync(
                    predicate: rr => rr.RoundId == preliminaryRound.Id && rr.Status == "public");
                if (publishedRegistrationsCount > 0)
                {
                    throw new BadRequestException("Không thể thêm cá vào vòng thi đã được công khai.");
                }
                
                // 4️⃣ Kiểm tra danh sách cá hợp lệ
                if (registrationIds == null || !registrationIds.Any())
                {
                    throw new BadRequestException("Danh sách cá không được để trống.");
                }
                
                // 5️⃣ Lấy danh sách đăng ký đã check-in theo IDs được chỉ định
                var registrations = await registrationRepository.GetListAsync(
                    predicate: r => r.CompetitionCategoryId == categoryId && 
                                    registrationIds.Contains(r.Id) &&
                                    r.Status == RegistrationStatus.CheckIn.ToString().ToLower());

                if (!registrations.Any())
                {
                    throw new BadRequestException("Không tìm thấy đơn đăng ký hợp lệ đã check-in trong danh sách đã chỉ định.");
                }
                
                // Kiểm tra xem tất cả các ID có thuộc về hạng mục này không
                if (registrations.Count < registrationIds.Count)
                {
                    var foundIds = registrations.Select(r => r.Id).ToList();
                    var missingIds = registrationIds.Where(id => !foundIds.Contains(id)).ToList();
                    throw new BadRequestException($"Một số đơn đăng ký ({missingIds.Count}) không tồn tại hoặc không thuộc về hạng mục này hoặc chưa check-in.");
                }
                
                // 6️⃣ Kiểm tra xem có đơn đăng ký nào đã được gán vào vòng này chưa
                var existingInRound = await regisRoundRepository.GetListAsync(
                    predicate: rr => rr.RoundId == preliminaryRound.Id && registrationIds.Contains(rr.RegistrationId));
        
                if (existingInRound.Any())
                {
                    // Loại bỏ các đơn đăng ký đã được gán khỏi danh sách
                    var existingIds = existingInRound.Select(r => r.RegistrationId).ToList();
                    registrations = registrations.Where(r => !existingIds.Contains(r.Id)).ToList();
                    
                    if (!registrations.Any())
                    {
                        throw new BadRequestException("Tất cả các đơn đăng ký đã được gán vào vòng sơ khảo.");
                    }
                    
                    registrationIds = registrations.Select(r => r.Id).ToList();
                }

                // 7️⃣ Tạo bản ghi mới cho vòng Preliminary
                List<RegistrationRound> newRegisRounds = new();

                foreach (var registration in registrations)
                {
                    newRegisRounds.Add(new RegistrationRound
                    {
                        Id = Guid.NewGuid(),
                        RegistrationId = registration.Id,
                        RoundId = preliminaryRound.Id,
                        CheckInTime = VietNamTimeUtil.GetVietnamTime(),
                        Status = "unpublic",
                        CreatedAt = VietNamTimeUtil.GetVietnamTime()
                    });
                    
                    registration.Status = "competition";
                    registrationRepository.UpdateAsync(registration);
                }

                // 8️⃣ Chèn bản ghi mới vào bảng
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
    }
}