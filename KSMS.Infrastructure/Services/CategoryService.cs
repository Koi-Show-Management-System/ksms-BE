using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Entities;
using KSMS.Infrastructure.Database;
using System.Linq.Expressions;
using KSMS.Application.Extensions;
using KSMS.Domain.Dtos.Requests.Categorie;
using Microsoft.EntityFrameworkCore;
using Mapster;
using KSMS.Domain.Dtos.Responses.CompetitionCategory;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ShowStatus = KSMS.Domain.Enums.ShowStatus;

namespace KSMS.Infrastructure.Services
{
    public class CategoryService : BaseService<CategoryService>, ICategoryService
    {
        private readonly INotificationService _notificationService;
        
        public CategoryService(
            IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, 
            ILogger<CategoryService> logger, 
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService) 
            : base(unitOfWork, logger, httpContextAccessor)
        {
            _notificationService = notificationService;
        }

        public async Task CreateCompetitionCategory(CreateCompetitionCategoryRequest request)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.KoiShowId);
            if (show == null)
            {
                throw new NotFoundException("Không tìm thấy triển lãm");
            }
            var existingCategory = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate: k =>
                k.Name.ToLower() == request.Name.ToLower() && k.KoiShowId == request.KoiShowId);
            if (existingCategory is not null)
            {
                throw new BadRequestException("Tên hạng mục đã tồn tại trong triển lãm này. Vui lòng chọn tên khác");
            }
            if (request.CreateCompetionCategoryVarieties.Any())
            {
                foreach (var varietyId in request.CreateCompetionCategoryVarieties)
                {
                    var variety = await _unitOfWork.GetRepository<Variety>()
                        .SingleOrDefaultAsync(predicate: x => x.Id == varietyId);
                    if (variety == null)
                    {
                        throw new NotFoundException("Không tìm thấy giống cá có Id: " + varietyId);
                    }
                }
            }

            if (request.CreateCriteriaCompetitionCategoryRequests.Any())
            {
                foreach (var criteriaCategory in request.CreateCriteriaCompetitionCategoryRequests)
                {
                    var criteria = await _unitOfWork.GetRepository<Criterion>().SingleOrDefaultAsync(predicate: x => x.Id == criteriaCategory.CriteriaId);
                    if (criteria == null)
                    {
                        throw new NotFoundException("Không tìm thấy tiêu chí có Id: " + criteriaCategory.CriteriaId);
                    }
                }
            }
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var category = request.Adapt<CompetitionCategory>();
                await _unitOfWork.GetRepository<CompetitionCategory>().InsertAsync(category);
                await _unitOfWork.CommitAsync();
                if (request.CreateRoundRequests.Any())
                {
                    var rounds = request.CreateRoundRequests.Select(r =>
                    {
                        var round = r.Adapt<Round>();
                        round.CompetitionCategoriesId = category.Id;
                        return round;
                    }).ToList();
                    await _unitOfWork.GetRepository<Round>().InsertRangeAsync(rounds);
                }
                if (request.CreateCompetionCategoryVarieties.Any())
                {
                    var categoryVarieties = request.CreateCompetionCategoryVarieties.Select(v => new CategoryVariety
                    {
                        CompetitionCategoryId = category.Id,
                        VarietyId = v
                    }).ToList();
                    await _unitOfWork.GetRepository<CategoryVariety>().InsertRangeAsync(categoryVarieties);
                }
                if (request.CreateAwardCateShowRequests.Any())
                {
                    var awards = request.CreateAwardCateShowRequests.Select(a =>
                    {
                        var award = a.Adapt<Award>();
                        award.CompetitionCategoriesId = category.Id;
                        return award;
                    }).ToList();
                    await _unitOfWork.GetRepository<Award>().InsertRangeAsync(awards);
                }
                if (request.CreateCriteriaCompetitionCategoryRequests.Any())
                {
                    var criteriaCategories = request.CreateCriteriaCompetitionCategoryRequests.Select(c =>
                    {
                        var criteria = c.Adapt<CriteriaCompetitionCategory>();
                        criteria.CompetitionCategoryId = category.Id;
                        return criteria;
                    }).ToList();
                    await _unitOfWork.GetRepository<CriteriaCompetitionCategory>().InsertRangeAsync(criteriaCategories);
                }
                if (request.CreateRefereeAssignmentRequests.Any())
                {
                    var refereeAssignments = request.CreateRefereeAssignmentRequests
                        .SelectMany(r => r.RoundTypes.Select(rt => new RefereeAssignment
                        {
                            RefereeAccountId = r.RefereeAccountId,
                            CompetitionCategoryId = category.Id,
                            RoundType = rt,
                            AssignedAt = VietNamTimeUtil.GetVietnamTime(),
                            AssignedBy = GetIdFromJwt()
                        })).ToList();
                    await _unitOfWork.GetRepository<RefereeAssignment>().InsertRangeAsync(refereeAssignments);
                }

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateCompetitionCategory(Guid id, UpdateCompetitionCategoryRequest request)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                include: query => query
                    .Include(x => x.Awards)
                    .Include(x => x.CategoryVarieties)
                    .Include(x => x.CriteriaCompetitionCategories)
                    .Include(x => x.RefereeAssignments)
                    .Include(x => x.Rounds));
            if (category == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục");
            }
            var show = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: x => x.Id == request.KoiShowId);
            if (show == null)
            {
                throw new NotFoundException("Không tìm thấy cuộc thi");
            }
            var existingCategory = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate: k =>
                k.Name.ToLower() == request.Name.ToLower() && k.Id != id && k.KoiShowId == request.KoiShowId);  
            if (existingCategory is not null)
            {
                throw new BadRequestException("Tên hạng mục đã tồn tại trong triển lãm này. Vui lòng chọn tên khác");
            }
            if (request.CreateCompetionCategoryVarieties.Any())
            {
                foreach (var varietyId in request.CreateCompetionCategoryVarieties)
                {
                    var variety = await _unitOfWork.GetRepository<Variety>()
                        .SingleOrDefaultAsync(predicate: x => x.Id == varietyId);
                    if (variety == null)
                    {
                        throw new NotFoundException("Không tìm thấy giống cá có Id: " + varietyId);
                    }
                }
            }

            if (request.CreateCriteriaCompetitionCategoryRequests.Any())
            {
                foreach (var criteriaCategory in request.CreateCriteriaCompetitionCategoryRequests)
                {
                    var criteria = await _unitOfWork.GetRepository<Criterion>().SingleOrDefaultAsync(predicate: x => x.Id == criteriaCategory.CriteriaId);
                    if (criteria == null)
                    {
                        throw new NotFoundException("Không tìm thấy tiêu chí có Id: " + criteriaCategory.CriteriaId);
                    }
                }
            }
            var roundIds = category.Rounds.Select(x => x.Id).ToList();
            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>()
                .GetListAsync(predicate: x => roundIds.Contains(x.RoundId));
            if (registrationRounds.Any())
            {
                throw new BadRequestException("Không thể xóa vòng thi đã có đăng ký");
            }
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
               
                if (category.Awards.Any())
                {
                    _unitOfWork.GetRepository<Award>().DeleteRangeAsync(category.Awards);
                }
                if (category.CategoryVarieties.Any())
                {
                    _unitOfWork.GetRepository<CategoryVariety>().DeleteRangeAsync(category.CategoryVarieties);
                }
                if (category.CriteriaCompetitionCategories.Any())
                {
                    _unitOfWork.GetRepository<CriteriaCompetitionCategory>().DeleteRangeAsync(category.CriteriaCompetitionCategories);
                }
                if (category.RefereeAssignments.Any())
                {
                    _unitOfWork.GetRepository<RefereeAssignment>().DeleteRangeAsync(category.RefereeAssignments);
                }
                if (category.Rounds.Any())
                {
                    _unitOfWork.GetRepository<Round>().DeleteRangeAsync(category.Rounds);
                }

                request.Adapt(category);
                await _unitOfWork.CommitAsync();
                if (request.CreateRoundRequests.Any())
                {
                    var rounds = request.CreateRoundRequests.Select(r =>
                    {
                        var round = r.Adapt<Round>();
                        round.CompetitionCategoriesId = category.Id;
                        return round;
                    }).ToList();
                    await _unitOfWork.GetRepository<Round>().InsertRangeAsync(rounds);
                }
                if (request.CreateCompetionCategoryVarieties.Any())
                {
                    var categoryVarieties = request.CreateCompetionCategoryVarieties.Select(v => new CategoryVariety
                    {
                        CompetitionCategoryId = category.Id,
                        VarietyId = v
                    }).ToList();
                    await _unitOfWork.GetRepository<CategoryVariety>().InsertRangeAsync(categoryVarieties);
                }

                if (request.CreateAwardCateShowRequests.Any())
                {
                    var awards = request.CreateAwardCateShowRequests.Select(a =>
                    {
                        var award = a.Adapt<Award>();
                        award.CompetitionCategoriesId = category.Id;
                        return award;
                    }).ToList();
                    await _unitOfWork.GetRepository<Award>().InsertRangeAsync(awards);
                }
                if (request.CreateCriteriaCompetitionCategoryRequests.Any())
                {
                    var criteriaCategories = request.CreateCriteriaCompetitionCategoryRequests.Select(c =>
                    {
                        var criteria = c.Adapt<CriteriaCompetitionCategory>();
                        criteria.CompetitionCategoryId = category.Id;
                        return criteria;
                    }).ToList();
                    await _unitOfWork.GetRepository<CriteriaCompetitionCategory>().InsertRangeAsync(criteriaCategories);
                }
                if (request.CreateRefereeAssignmentRequests.Any())
                {
                    var refereeAssignments = request.CreateRefereeAssignmentRequests
                        .SelectMany(r => r.RoundTypes.Select(rt => new RefereeAssignment
                        {
                            RefereeAccountId = r.RefereeAccountId,
                            CompetitionCategoryId = category.Id,
                            RoundType = rt,
                            AssignedAt = VietNamTimeUtil.GetVietnamTime(),
                            AssignedBy = GetIdFromJwt()
                        })).ToList();
                    await _unitOfWork.GetRepository<RefereeAssignment>().InsertRangeAsync(refereeAssignments);
                }

                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }
        
        
        
        public async Task<GetCompetitionCategoryDetailResponse> GetCompetitionCategoryDetailById(Guid id)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(predicate:
                x => x.Id == id,
                include: query => query
                    .Include(x => x.Awards)   
                    .Include(x => x.CategoryVarieties)
                        .ThenInclude(x => x.Variety)
                    .Include(x => x.CriteriaCompetitionCategories)
                        .ThenInclude(x => x.Criteria)
                    .Include(x => x.RefereeAssignments)
                        .ThenInclude(x => x.RefereeAccount)
                    .Include(x => x.RefereeAssignments)
                        .ThenInclude(x => x.AssignedByNavigation)
                    .Include(x => x.Rounds));
                
            if (category == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục");
            }
            return category.Adapt<GetCompetitionCategoryDetailResponse>();
        }

        public async Task<Paginate<GetPageCompetitionCategoryResponse>> GetPagedCompetitionCategory(Guid showId, bool? hasTank, int page, int size)
        {
            var role = GetRoleFromJwt();
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == showId);
            if (show == null)
            {
                throw new NotFoundException("Không tìm thấy cuộc thi");
            }
            Expression<Func<CompetitionCategory, bool>> filterQuery = category => category.KoiShowId == showId;
            // if (role is "Guest" or "Member")
            // {
            //     filterQuery = filterQuery.And(category => category.Status == CategoryStatus.Active.ToString().ToLower());
            // }
            if (role is "Referee")
            {
                filterQuery = filterQuery.AndAlso(category => category.RefereeAssignments.Any(x => x.RefereeAccountId == GetIdFromJwt()));
            }
            if (hasTank.HasValue)
            {
                filterQuery = filterQuery.AndAlso(category => category.HasTank == hasTank);
            }
            var categories = await _unitOfWork.GetRepository<CompetitionCategory>().GetPagingListAsync(predicate:
                filterQuery,
                orderBy: query => query.OrderBy(x => x.Name),
                include: query => query.AsSplitQuery()
                    .Include(x => x.RefereeAssignments)
                    .Include(x => x.CategoryVarieties)
                        .ThenInclude(x => x.Variety),
                page: page,
                size: size
            );
            
            var response = categories.Adapt<Paginate<GetPageCompetitionCategoryResponse>>();
            foreach (var category in response.Items)
            {
                var categoryEntity = categories.Items.FirstOrDefault(x => x.Id == category.Id);
                if (categoryEntity != null)
                {
                    category.Varieties = categoryEntity.CategoryVarieties.Select(cv => cv.Variety.Name).ToList();
                }
            }
            return response;
        }
        
        public async Task DeleteCategoryAsync(Guid id)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                include: query => query
                    .Include(x => x.Awards)
                    .Include(x => x.CategoryVarieties)
                    .Include(x => x.CriteriaCompetitionCategories)
                    .Include(x => x.RefereeAssignments)
                    .Include(x => x.Rounds)
                    .Include(x => x.KoiShow));
                    
            if (category == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục");
            }
            
            // Kiểm tra trạng thái của show
            var showStatus = category.KoiShow.Status.ToLower();
            if (showStatus != ShowStatus.Pending.ToString().ToLower() && 
                showStatus != ShowStatus.InternalPublished.ToString().ToLower())
            {
                throw new BadRequestException("Không thể xóa hạng mục khi triển lãm không ở trạng thái 'Đang chờ duyệt' hoặc 'Đã duyệt nội bộ'");
            }
            
            // Kiểm tra xem các vòng thi đã có đăng ký chưa
            var roundIds = category.Rounds.Select(x => x.Id).ToList();
            var registrationRounds = await _unitOfWork.GetRepository<RegistrationRound>()
                .GetListAsync(predicate: x => roundIds.Contains(x.RoundId));
                
            if (registrationRounds.Any())
            {
                throw new BadRequestException("Không thể xóa hạng mục đã có đăng ký vòng thi");
            }
            
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Xóa các bảng liên kết
                if (category.Awards.Any())
                {
                    _unitOfWork.GetRepository<Award>().DeleteRangeAsync(category.Awards);
                }
                
                if (category.CategoryVarieties.Any())
                {
                    _unitOfWork.GetRepository<CategoryVariety>().DeleteRangeAsync(category.CategoryVarieties);
                }
                
                if (category.CriteriaCompetitionCategories.Any())
                {
                    _unitOfWork.GetRepository<CriteriaCompetitionCategory>().DeleteRangeAsync(category.CriteriaCompetitionCategories);
                }
                
                if (category.RefereeAssignments.Any())
                {
                    _unitOfWork.GetRepository<RefereeAssignment>().DeleteRangeAsync(category.RefereeAssignments);
                }
                
                if (category.Rounds.Any())
                {
                    _unitOfWork.GetRepository<Round>().DeleteRangeAsync(category.Rounds);
                }
                
                // Xóa category
                _unitOfWork.GetRepository<CompetitionCategory>().DeleteAsync(category);
                
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task CancelCategoryAsync(Guid id, string reason)
        {
            var category = await _unitOfWork.GetRepository<CompetitionCategory>().SingleOrDefaultAsync(
                predicate: x => x.Id == id,
                include: query => query
                    .Include(x => x.Rounds)
                    .Include(x => x.KoiShow));
                    
            if (category == null)
            {
                throw new NotFoundException("Không tìm thấy hạng mục");
            }
            
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new BadRequestException("Lý do hủy hạng mục không được để trống");
            }
            
            // Kiểm tra xem cuộc thi đã diễn ra chưa
            var currentTime = VietNamTimeUtil.GetVietnamTime();
            if (category.KoiShow.StartDate <= currentTime)
            {
                throw new BadRequestException("Không thể hủy hạng mục khi cuộc thi đã bắt đầu");
            }
            
            // Cập nhật trạng thái hạng mục thành cancelled
            category.Status = CategoryStatus.Cancelled.ToString().ToLower();
            
            _unitOfWork.GetRepository<CompetitionCategory>().UpdateAsync(category);
            await _unitOfWork.CommitAsync();
            
            // Lấy danh sách tất cả các đăng ký cho hạng mục này có trạng thái là confirmed hoặc pending
            var registrations = await _unitOfWork.GetRepository<Registration>()
                .GetListAsync(
                    include: query => query
                        .Include(r => r.Account),
                    predicate: r => r.CompetitionCategoryId == id && 
                                   (r.Status == RegistrationStatus.Confirmed.ToString().ToLower() ||
                                    r.Status == RegistrationStatus.Pending.ToString().ToLower()));
            
            // Nhóm các đăng ký theo account
            var registrationsByAccount = registrations.GroupBy(r => r.AccountId);
            
            // Xử lý từng người dùng đã đăng ký
            foreach (var accountGroup in registrationsByAccount)
            {
                var accountId = accountGroup.Key;
                var accountRegistrations = accountGroup.ToList();
                
                foreach (var registration in accountRegistrations)
                {
                    // Cập nhật trạng thái đăng ký thành pending refund
                    registration.Status = RegistrationStatus.PendingRefund.ToString().ToLower();
                    _unitOfWork.GetRepository<Registration>().UpdateAsync(registration);
                }
                
                // Gửi thông báo cho người dùng về việc hủy hạng mục
                await _notificationService.SendNotification(
                    accountId,
                    $"Hạng mục {category.Name} đã bị hủy - Đang chờ xử lí hoàn tiền",
                    $"Hạng mục {category.Name} của cuộc thi {category.KoiShow.Name} đã bị hủy vì lý do: {reason}. Phí đăng ký của bạn đang được xử lí hoàn trả trong 3-5 ngày làm việc.",
                    NotificationType.Registration);
            }
            
            await _unitOfWork.CommitAsync();
        }
    }
}