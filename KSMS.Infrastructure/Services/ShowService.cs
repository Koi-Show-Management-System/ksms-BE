using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Requests.Show;
using KSMS.Domain.Dtos.Responses.KoiShow;

using KSMS.Domain.Entities;
using KSMS.Domain.Exceptions;
using KSMS.Infrastructure.Database;
using Mapster;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KSMS.Infrastructure.Utils;
using Microsoft.AspNetCore.Http;
using static KSMS.Infrastructure.Utils.MailUtil;
using KSMS.Domain.Pagination;
using System.Linq.Expressions;
using Hangfire;
using KSMS.Application.Extensions;
using KSMS.Domain.Dtos;
using KSMS.Domain.Dtos.Requests.ShowRule;
using KSMS.Domain.Enums;
using ShowStatus = KSMS.Domain.Entities.ShowStatus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace KSMS.Infrastructure.Services
{
    public class ShowService : BaseService<ShowService>, IShowService
    {
        private readonly INotificationService _notificationService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IEmailService _emailService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMemoryCache _cache;
        private readonly Dictionary<ShowProgress, string> _notificationMessages;

        public ShowService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork,
            ILogger<ShowService> logger, IHttpContextAccessor httpContextAccessor, INotificationService notificationService, IBackgroundJobClient backgroundJobClient, IEmailService emailService,
            IServiceScopeFactory serviceScopeFactory, IMemoryCache cache)
            : base(unitOfWork, logger, httpContextAccessor)
        {
            _notificationService = notificationService;
            _backgroundJobClient = backgroundJobClient;
            _emailService = emailService;
            _serviceScopeFactory = serviceScopeFactory;
            _cache = cache;
            _notificationMessages = new Dictionary<ShowProgress, string>
            {
                { ShowProgress.RegistrationOpen, "Đăng ký tham gia triển lãm đã được mở!" },
                { ShowProgress.KoiCheckIn, "Triển lãm đang diễn ra!" },
                { ShowProgress.TicketCheckIn, "Triển lãm đang diễn ra!" },
                { ShowProgress.Preliminary, "Triển lãm đang diễn ra!" },
                { ShowProgress.Evaluation, "Triển lãm đang diễn ra!" },
                { ShowProgress.Final, "Triển lãm đang diễn ra!" },
                { ShowProgress.Exhibition, "Triển lãm đang diễn ra!" },
                { ShowProgress.PublicResult, "Triển lãm đang diễn ra!" },
                { ShowProgress.Award, "Triển lãm đang diễn ra!" },
                { ShowProgress.Finished, "Triển lãm đã kết thúc!" }
            };
        }

        public async Task UpdateShowV2(Guid id, UpdateShowRequestV2 request)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: query => query.Include(x => x.Sponsors)
                    .Include(x => x.TicketTypes)
                    .Include(x => x.ShowRules)
                    .Include(x => x.ShowStatuses));
            if (show is null)
            {
                throw new NotFoundException("Không tìm thấy triển lãm");
            }
            if (request.Name is not null)
            {
                var existingShow = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: k =>
                    k.Name.ToLower() == request.Name.ToLower());

                if (existingShow is not null && existingShow.Id != show.Id)
                {
                    throw new BadRequestException("Tên triển lãm này đã tồn tại. Vui lòng chọn tên khác");
                }
            }
            request.Adapt(show);
            _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
            await _unitOfWork.CommitAsync();
            
        }

        public async Task<GetKoiShowDetailResponse> GetShowDetailByIdAsync(Guid id)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: x => x.Id == id,
                include: query => query.Include(x => x.ShowRules)
                    .Include(x => x.ShowStatuses)
                    .Include(x => x.Sponsors)
                    .Include(x => x.TicketTypes)
                    .Include(x => x.CompetitionCategories)
                    .ThenInclude(c => c.CriteriaCompetitionCategories)
                    .ThenInclude(cc => cc.Criteria));
            if (show is null)
            {
                throw new NotFoundException("Không tìm thấy triển lãm");
            }
            var response = show.Adapt<GetKoiShowDetailResponse>();
            var criteria = show.CompetitionCategories
                .SelectMany(c => c.CriteriaCompetitionCategories
                    .Select(cc => cc.Criteria.Name)).Distinct().ToList();
            response.Criteria = criteria.ToList();
            return response;
        }
        
        public async Task CreateShowAsync(CreateShowRequest createShowRequest)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>().SingleOrDefaultAsync(predicate: k =>
                k.Name.ToLower() == createShowRequest.Name.ToLower());
            if (show is not null)
            {
                throw new BadRequestException("Tên triển lãm đã tồn tại. Vui lòng chọn tên khác");
            }
            if (string.IsNullOrWhiteSpace(createShowRequest.Name))
                throw new BadRequestException("Tên triển lãm không được để trống");

            if (createShowRequest.StartDate >= createShowRequest.EndDate)
                throw new BadRequestException("Ngày bắt đầu phải sớm hơn ngày kết thúc");
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            var categoryRepository = _unitOfWork.GetRepository<CompetitionCategory>();
            var sponsorRepository = _unitOfWork.GetRepository<Sponsor>();
            var showStaffRepository = _unitOfWork.GetRepository<ShowStaff>();
            var showRuleRepository = _unitOfWork.GetRepository<ShowRule>();
            var showStatusRepository = _unitOfWork.GetRepository<ShowStatus>();
            var roundRepository = _unitOfWork.GetRepository<Round>();
            var criteriaGroupRepository = _unitOfWork.GetRepository<CriteriaCompetitionCategory>();
            var criteriaRepository = _unitOfWork.GetRepository<Criterion>();
            var refereeAssignmentRepository = _unitOfWork.GetRepository<RefereeAssignment>();
            var accountRepository = _unitOfWork.GetRepository<Account>();
            var categoryVarietyRepository = _unitOfWork.GetRepository<CategoryVariety>();
            var awardRepository = _unitOfWork.GetRepository<Award>();
    
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try 
            {
                // Tạo show
                var newShow = createShowRequest.Adapt<KoiShow>();
                newShow.CreatedAt = DateTime.UtcNow;
                await showRepository.InsertAsync(newShow);
                await _unitOfWork.CommitAsync();
                var showId = newShow.Id;
                
                // Xử lý show rules
                if (createShowRequest.CreateShowRuleRequests.Any())
                {
                    var rules = createShowRequest.CreateShowRuleRequests
                        .Select(r => {
                            var rule = r.Adapt<ShowRule>();
                            rule.KoiShowId = showId;
                            return rule;
                        }).ToList();
                    
                    await showRuleRepository.InsertRangeAsync(rules);
                }
                
                // Xử lý show statuses
                if (createShowRequest.CreateShowStatusRequests.Any())
                {
                    var statuses = createShowRequest.CreateShowStatusRequests
                        .Select(s => {
                            var status = s.Adapt<ShowStatus>();
                            status.KoiShowId = showId;
                            return status;
                        }).ToList();
                    
                    await showStatusRepository.InsertRangeAsync(statuses);
                }
                
                // Xử lý sponsors
                if (createShowRequest.CreateSponsorRequests.Any())
                {
                    var sponsors = createShowRequest.CreateSponsorRequests
                        .Select(s => {
                            var sponsor = s.Adapt<Sponsor>();
                            sponsor.KoiShowId = showId;
                            return sponsor;
                        }).ToList();
                    
                    await sponsorRepository.InsertRangeAsync(sponsors);
                }
                
                // Xử lý ticket types
                if (createShowRequest.CreateTicketTypeRequests.Any())
                {
                    var ticketTypes = createShowRequest.CreateTicketTypeRequests
                        .Select(t => {
                            var ticket = t.Adapt<TicketType>();
                            ticket.KoiShowId = showId;
                            return ticket;
                        }).ToList();
                    
                    await _unitOfWork.GetRepository<TicketType>().InsertRangeAsync(ticketTypes);
                }
                
                // Xử lý staff assignments
                var currentUserId = GetIdFromJwt();
                var staffEmails = new List<(Guid id, string fullName, string email)>();
                
                if (createShowRequest.AssignStaffRequests.Any())
                {
                    var staffs = createShowRequest.AssignStaffRequests
                        .Select(staffId => new ShowStaff {
                            KoiShowId = showId,
                            AccountId = staffId,
                            AssignedBy = currentUserId,
                            AssignedAt = DateTime.UtcNow
                        }).ToList();
                    
                    await showStaffRepository.InsertRangeAsync(staffs);
                    
                    // Lấy thông tin staff để gửi email sau
                    var staffAccounts = await accountRepository.GetListAsync(
                        predicate: a => createShowRequest.AssignStaffRequests.Contains(a.Id));
                    
                    staffEmails.AddRange(staffAccounts.Select(a => (a.Id, a.FullName, a.Email)));
                }
                
                // Xử lý manager assignments
                if (createShowRequest.AssignManagerRequests.Any())
                {
                    var managers = createShowRequest.AssignManagerRequests
                        .Select(managerId => new ShowStaff {
                            KoiShowId = showId,
                            AccountId = managerId,
                            AssignedBy = currentUserId,
                            AssignedAt = DateTime.UtcNow
                        }).ToList();
                    
                    await showStaffRepository.InsertRangeAsync(managers);
                    
                    // Lấy thông tin manager để gửi email sau
                    var managerAccounts = await accountRepository.GetListAsync(
                        predicate: a => createShowRequest.AssignManagerRequests.Contains(a.Id));
                    
                    staffEmails.AddRange(managerAccounts.Select(a => (a.Id, a.FullName, a.Email)));
                }
                
                // Xử lý categories
                if (createShowRequest.CreateCategorieShowRequests.Any())
                {
                    foreach (var categoryRequest in createShowRequest.CreateCategorieShowRequests)
                    {
                        // Tạo category
                        var category = categoryRequest.Adapt<CompetitionCategory>();
                        category.KoiShowId = showId;
                        await categoryRepository.InsertAsync(category);
                        await _unitOfWork.CommitAsync();
                        // Xử lý varieties
                        if (categoryRequest.CreateCompetionCategoryVarieties.Any())
                        {
                            // Kiểm tra tất cả variety ID có tồn tại
                            var varieties = await _unitOfWork.GetRepository<Variety>().GetListAsync(
                                predicate: v => categoryRequest.CreateCompetionCategoryVarieties.Contains(v.Id));
                            
                            if (varieties.Count != categoryRequest.CreateCompetionCategoryVarieties.Count)
                            {
                                var foundIds = varieties.Select(v => v.Id).ToList();
                                var missingIds = categoryRequest.CreateCompetionCategoryVarieties
                                    .Where(id => !foundIds.Contains(id))
                                    .ToList();
                                throw new NotFoundException($"Không tìm thấy giống cá có ID: {string.Join(", ", missingIds)}");
                            }
                            
                            var categoryVarieties = varieties.Select(v => new CategoryVariety {
                                CompetitionCategoryId = category.Id,
                                VarietyId = v.Id
                            }).ToList();
                            
                            await categoryVarietyRepository.InsertRangeAsync(categoryVarieties);
                        }
                        
                        // Xử lý criteria
                        if (categoryRequest.CreateCriteriaCompetitionCategoryRequests.Any())
                        {
                            var criteriaIds = categoryRequest.CreateCriteriaCompetitionCategoryRequests
                                .Select(c => c.CriteriaId)
                                .Distinct()
                                .ToList();
                            
                            var criteria = await criteriaRepository.GetListAsync(
                                predicate: c => criteriaIds.Contains(c.Id));
                            
                            if (criteria.Count != criteriaIds.Count)
                            {
                                var foundIds = criteria.Select(c => c.Id).ToList();
                                var missingIds = criteriaIds
                                    .Where(id => !foundIds.Contains(id))
                                    .ToList();
                                throw new BadRequestException($"Không tìm thấy tiêu chí có ID: {string.Join(", ", missingIds)}");
                            }
                            
                            var criteriaGroups = categoryRequest.CreateCriteriaCompetitionCategoryRequests
                                .Select(g => {
                                    var group = g.Adapt<CriteriaCompetitionCategory>();
                                    group.CompetitionCategoryId = category.Id;
                                    return group;
                                }).ToList();
                            
                            await criteriaGroupRepository.InsertRangeAsync(criteriaGroups);
                        }
                        
                        // Xử lý rounds
                        if (categoryRequest.CreateRoundRequests.Any())
                        {
                            var rounds = categoryRequest.CreateRoundRequests
                                .Select(r => {
                                    var round = r.Adapt<Round>();
                                    round.CompetitionCategoriesId = category.Id;
                                    return round;
                                }).ToList();
                            
                            await roundRepository.InsertRangeAsync(rounds);
                        }
                        
                        // Xử lý referee assignments
                        if (categoryRequest.CreateRefereeAssignmentRequests.Any())
                        {
                            var refereeAssignments = new List<RefereeAssignment>();
                            var refereeIds = categoryRequest.CreateRefereeAssignmentRequests
                                .Select(r => r.RefereeAccountId)
                                .Distinct()
                                .ToList();
                            
                            var refereeAccounts = await accountRepository.GetListAsync(
                                predicate: a => refereeIds.Contains(a.Id));
                            
                            foreach (var refereeRequest in categoryRequest.CreateRefereeAssignmentRequests)
                            {
                                if (!refereeRequest.RoundTypes.Any())
                                {
                                    throw new BadRequestException("Bạn cần chọn loại vòng thi cho trọng tài");
                                }
                                foreach (var roundType in refereeRequest.RoundTypes)
                                {
                                    refereeAssignments.Add(new RefereeAssignment {
                                        RefereeAccountId = refereeRequest.RefereeAccountId,
                                        CompetitionCategoryId = category.Id,
                                        RoundType = roundType,
                                        AssignedAt = DateTime.UtcNow,
                                        AssignedBy = currentUserId
                                    });
                                }
                            }
                            
                            await refereeAssignmentRepository.InsertRangeAsync(refereeAssignments);
                            
                            // Lưu thông tin để gửi email sau
                            staffEmails.AddRange(refereeAccounts.Select(a => (a.Id, a.FullName ?? "NoName", a.Email ?? "NoEmail")));
                        }
                        
                        // Xử lý awards
                        if (categoryRequest.CreateAwardCateShowRequests.Any())
                        {
                            var awards = categoryRequest.CreateAwardCateShowRequests
                                .Select(a => {
                                    var award = a.Adapt<Award>();
                                    award.CompetitionCategoriesId = category.Id;
                                    return award;
                                }).ToList();
                            
                            await awardRepository.InsertRangeAsync(awards);
                        }
                    }
                }
                
                // Commit một lần
                await _unitOfWork.CommitAsync();
                await transaction.CommitAsync();
                
                // Gửi email (bất đồng bộ sau khi transaction đã commit)
                // Task.Run(() => {
                //     foreach (var staff in staffEmails)
                //     {
                //         string emailBody = ContentMailUtil.StaffRoleNotification(
                //             staff.fullName, 
                //             createShowRequest.Name, 
                //             staff.email, 
                //             "DefaultPassword123"
                //         );
                //         MailUtil.SendEmail(
                //             staff.email, 
                //             "[KOI SHOW SYSTEM] New Role Assigned", 
                //             emailBody, 
                //             null
                //         );
                //     }
                // });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Paginate<GetMemberRegisterShowResponse>> GetMemberRegisterShowAsync(Domain.Enums.ShowStatus? showStatus, int page, int size)
        {
            var currentAccount = GetIdFromJwt();
            Expression<Func<Registration, bool>> predicate = r => r.AccountId == currentAccount;
            if (showStatus.HasValue)
            {
                var status = showStatus.Value.ToString().ToLower();
                predicate = predicate.AndAlso(r => r.KoiShow.Status == status);
            }

            var registeredShows = await _unitOfWork.GetRepository<Registration>()
                .GetListAsync(
                    predicate: predicate,
                    include: query => query.AsSplitQuery()
                        .Include(r => r.KoiShow)
                        .Include(r => r.CompetitionCategory),
                    orderBy: q => q.OrderByDescending(r => r.CreatedAt));
            var groupedByShow = registeredShows
                .GroupBy(r => r.KoiShowId)
                .Select(group => new GetMemberRegisterShowResponse
                {
                    Id = group.Key,
                    ShowName = group.First().KoiShow.Name,
                    ImageUrl = group.First().KoiShow.ImgUrl,
                    Location = group.First().KoiShow.Location,
                    StartDate = group.First().KoiShow.StartDate,
                    EndDate = group.First().KoiShow.EndDate,
                    Status = group.First().KoiShow.Status,
                    CancellationReason = group.First().KoiShow.CancellationReason
                }).ToList();
                var result = new Paginate<GetMemberRegisterShowResponse>(groupedByShow, page, size, 1);
                return result;
        }

        public async Task CancelShowAsync(Guid id, Domain.Enums.ShowStatus status, string? reason)
        {
            var show = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: s => s.Id == id);
            if (show is null)
            {
                throw new NotFoundException("Không tìm thấy triển lãm");
            }
            show.Status = status switch
            {
                Domain.Enums.ShowStatus.Cancelled =>  Domain.Enums.ShowStatus.Cancelled.ToString().ToLower(),
                Domain.Enums.ShowStatus.InternalPublished =>  Domain.Enums.ShowStatus.InternalPublished.ToString().ToLower(),
                Domain.Enums.ShowStatus.InProgress =>  Domain.Enums.ShowStatus.InProgress.ToString().ToLower(),
                Domain.Enums.ShowStatus.Pending =>  Domain.Enums.ShowStatus.Pending.ToString().ToLower(),
                Domain.Enums.ShowStatus.Finished =>  Domain.Enums.ShowStatus.Finished.ToString().ToLower(),
                Domain.Enums.ShowStatus.Published =>  Domain.Enums.ShowStatus.Published.ToString().ToLower(),
                Domain.Enums.ShowStatus.Upcoming =>  Domain.Enums.ShowStatus.Upcoming.ToString().ToLower(),
                _ => show.Status
            };

            if (show.Status == Domain.Enums.ShowStatus.Cancelled.ToString().ToLower())
            {
                if (reason is null)
                {
                    throw new BadRequestException("Lý do hủy triển lãm không được để trống");
                }
                show.CancellationReason = reason;
                _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
                await _unitOfWork.CommitAsync();
                
                // Hủy tất cả công việc Hangfire đã lên lịch cho show này
                // Lưu ý: Bạn cần đảm bảo đã thiết lập mô hình của Hangfire để hỗ trợ việc này
                _backgroundJobClient.Enqueue(() => CancelScheduledShowJobs(show.Id));
                
                var registrations = await _unitOfWork.GetRepository<Registration>()
                    .GetListAsync(
                        selector: r => new { r.Id, r.AccountId, r.Status },
                        predicate: r => r.KoiShowId == id);
                var registrationsByAccount = registrations
                    .Where(r => r.Status == RegistrationStatus.Confirmed.ToString().ToLower() || 
                                r.Status == RegistrationStatus.Pending.ToString().ToLower())
                    .GroupBy(r => r.AccountId);

                foreach (var accountGroup in registrationsByAccount)
                {
                    foreach (var registration in accountGroup)
                    {
                        var reg = await _unitOfWork.GetRepository<Registration>()
                            .SingleOrDefaultAsync(predicate: r => r.Id == registration.Id);
                        if (reg != null)
                        {
                            reg.Status = RegistrationStatus.PendingRefund.ToString().ToLower();
                            _unitOfWork.GetRepository<Registration>().UpdateAsync(reg);
                        }
                    }
                    await _notificationService.SendNotification(
                        accountGroup.Key,
                        "Triển lãm đã bị hủy - Đang chờ xử lí hoàn tiền",
                        $"Triển lãm {show.Name} đã bị hủy. Phí đăng ký của bạn đang được xử lí hoàn trả trong 3-5 ngày làm việc.",
                        NotificationType.Registration);
                }
                await _unitOfWork.CommitAsync();

                // Handle ticket orders
                var ticketOrders = await _unitOfWork.GetRepository<TicketOrder>()
                    .GetListAsync(
                        selector: t => new { t.Id, t.AccountId },
                        include: query => query
                            .Include(t => t.TicketOrderDetails)
                                .ThenInclude(tod => tod.Tickets),
                        predicate: t => t.TicketOrderDetails
                            .Any(tod => tod.TicketType.KoiShowId == id));

                // Group ticket orders by AccountId
                var ticketOrdersByAccount = ticketOrders.GroupBy(t => t.AccountId);

                foreach (var accountGroup in ticketOrdersByAccount)
                {
                    // Process all tickets in all orders of this account
                    foreach (var order in accountGroup)
                    {
                        var tickets = await _unitOfWork.GetRepository<Ticket>()
                            .GetListAsync(
                                predicate: t => t.TicketOrderDetail.TicketOrderId == order.Id &&
                                              t.Status == TicketStatus.Sold.ToString().ToLower());
                        
                        foreach (var ticket in tickets)
                        {
                            ticket.Status = TicketStatus.Cancelled.ToString().ToLower();
                            _unitOfWork.GetRepository<Ticket>().UpdateAsync(ticket);
                        }
                    }

                    // Send a single notification per account
                    await _notificationService.SendNotification(
                        accountGroup.Key,
                        "Triển lãm đã bị hủy - Hoàn tiền vé",
                        $"Triển lãm {show.Name} đã bị hủy. Tiền vé của bạn sẽ được hoàn trả trong 3-5 ngày làm việc.",
                        NotificationType.System);
                }
                await _unitOfWork.CommitAsync();

                // Notify all members
                var memberIds = await _unitOfWork.GetRepository<Account>()
                    .GetListAsync(
                        selector: a => a.Id,
                        predicate: a => a.Role == RoleName.Member.ToString().ToLower());

                foreach (var memberId in memberIds)
                {
                    await _notificationService.SendNotification(
                        memberId,
                        $"Triển lãm {show.Name} đã bị hủy",
                        $"Triển lãm {show.Name} đã bị hủy do lý do: {reason}. Nếu bạn đã đăng ký hoặc mua vé, chúng tôi sẽ liên hệ với bạn để hoàn tiền.",
                        NotificationType.System);
                }
            }

            if (show.Status == Domain.Enums.ShowStatus.InternalPublished.ToString().ToLower())
            {
                _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
                await _unitOfWork.CommitAsync();
                _backgroundJobClient.Enqueue(() => _emailService.SendShowStatusChange(show.Id));
            }

            if (show.Status == Domain.Enums.ShowStatus.Published.ToString().ToLower())
            {
                _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
                await _unitOfWork.CommitAsync();

                var memberIds = await _unitOfWork.GetRepository<Account>()
                    .GetListAsync(
                        selector: a => a.Id,
                        predicate: a => a.Role == RoleName.Member.ToString().ToLower());

                foreach (var memberId in memberIds)
                {
                    await _notificationService.SendNotification(
                        memberId,
                        $"Triển lãm {show.Name} đã chính thức được công bố",
                        $"Triển lãm {show.Name} đã được công bố. Bạn có thể theo dõi thời gian để mua vé và đăng ký tham gia ngay từ bây giờ.",
                        NotificationType.System);
                }
                _backgroundJobClient.Enqueue(() => _emailService.SendRefereeAssignmentNotification(show.Id));
                
                // THÊM MỚI: Lên lịch Hangfire cho các trạng thái show khi published
                await ScheduleShowStatusJobs(show.Id);
            }
        }

        public async Task<Paginate<PaginatedKoiShowResponse>> GetPagedShowsAsync(int page, int size)
        {
            var showRepository = _unitOfWork.GetRepository<KoiShow>();
            
            var role = GetRoleFromJwt(); 
            Expression<Func<KoiShow, bool>> filterQuery = show => true;
            if (role is "Guest" or "Member")
            {
                filterQuery = filterQuery.AndAlso(show => show.Status != Domain.Enums.ShowStatus.Pending.ToString().ToLower()
                && show.Status != Domain.Enums.ShowStatus.InternalPublished.ToString().ToLower());
            }
            else if (role is "Staff" or "Manager")
            {
                var accountId = GetIdFromJwt();
                filterQuery = filterQuery.AndAlso(show => 
                    show.ShowStaffs.Any(ss => ss.AccountId == accountId)  
                     && show.Status != Domain.Enums.ShowStatus.Pending.ToString().ToLower());
            }
            else if (role == "Referee")
            {
                var accountId = GetIdFromJwt();
                filterQuery = filterQuery.AndAlso(show =>
                    show.CompetitionCategories.Any(
                        c => c.RefereeAssignments.Any(ra => ra.RefereeAccountId == accountId)));
            }
            var pagedShows = await showRepository.GetPagingListAsync(
                predicate: filterQuery,
                orderBy: query => query.OrderBy(s => s.Name),
                include: query => query.AsSplitQuery().Include(s => s.ShowStatuses)
                    .Include(s => s.CompetitionCategories)
                    .ThenInclude(s => s.RefereeAssignments)
                    .Include(s => s.ShowStaffs),
                page: page,
                size: size
            );

            return pagedShows.Adapt<Paginate<PaginatedKoiShowResponse>>();
        }

        // Thêm phương thức mới để thay đổi trạng thái show status thủ công
        public async Task<bool> UpdateShowStatusManually(Guid koiShowId, string statusName, bool setActive)
        {
            try
            {
                // Lấy thông tin về status cần cập nhật
                var showStatus = await _unitOfWork.GetRepository<ShowStatus>()
                    .SingleOrDefaultAsync(
                        predicate: x => x.KoiShowId == koiShowId && x.StatusName == statusName,
                        include: query => query.Include(s => s.KoiShow));

                if (showStatus == null)
                {
                    throw new NotFoundException($"Không tìm thấy trạng thái '{statusName}' cho triển lãm này");
                }

                var showProgress = Enum.Parse<ShowProgress>(showStatus.StatusName);

                // Xử lý dựa trên setActive
                if (setActive && !showStatus.IsActive)
                {
                    // Gọi phương thức ActivateShowStatus để bảo đảm tính nhất quán
                    // Phương thức này sẽ tự động hủy kích hoạt trạng thái hiện tại
                    await ActivateShowStatus(koiShowId, showStatus.Id, statusName);
                    return true;
                }
                else if (!setActive && showStatus.IsActive)
                {
                    // Hủy kích hoạt status
                    showStatus.IsActive = false;
                    await UpdateShowStatusInternal(showStatus, showProgress);
                    return true;
                }

                // Nếu status đã ở trạng thái yêu cầu, trả về false
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật trạng thái triển lãm thủ công");
                throw;
            }
        }

        // Phương thức hỗ trợ để cập nhật show status và show
        private async Task UpdateShowStatusInternal(ShowStatus status, ShowProgress showProgress)
        {
            var showStatusRepo = _unitOfWork.GetRepository<ShowStatus>();
            var koiShowRepo = _unitOfWork.GetRepository<KoiShow>();

            showStatusRepo.UpdateAsync(status);

            if (status.IsActive)
            {
                var newStatus = showProgress switch
                {
                    ShowProgress.RegistrationOpen or 
                    ShowProgress.KoiCheckIn or
                    ShowProgress.TicketCheckIn => "upcoming",
                    
                    ShowProgress.Preliminary or 
                    ShowProgress.Evaluation or 
                    ShowProgress.Final or 
                    ShowProgress.Award or 
                    ShowProgress.PublicResult or 
                    ShowProgress.Exhibition => "inprogress",
                    
                    ShowProgress.Finished => "finished",
                    _ => status.KoiShow.Status
                };

                if (status.KoiShow.Status != newStatus)
                {
                    status.KoiShow.Status = newStatus;
                    koiShowRepo.UpdateAsync(status.KoiShow);
                }
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInformation(
                "Updated status for show {ShowId}: IsActive={IsActive}, Status={Status}", 
                status.KoiShowId, 
                status.IsActive, 
                status.KoiShow.Status
            );
        }

        // Gửi thông báo cho show status
        private async Task SendNotifications(ShowStatus status, ShowProgress showProgress)
        {
            try
            {
                var title = $"Thông báo: {status.KoiShow.Name}";
                var message = GetNotificationMessage(showProgress);
                var staffIds = await GetShowStaffIds(status.KoiShowId);

                switch (showProgress)
                {
                    case ShowProgress.RegistrationOpen:
                        // Gửi thông báo cho staff
                        await SendToStaff(status.KoiShowId, title, 
                            $"Triển lãm {status.KoiShow.Name} đã mở đăng ký. Vui lòng theo dõi và xử lý các đơn đăng ký.");
                        
                        // Gửi thông báo cho tất cả user khác
                        await SendToAllUsersExceptStaff(title, message, staffIds);
                        break;

                    case ShowProgress.Finished:
                        // Gửi thông báo kết thúc triển lãm
                        await SendToStaff(status.KoiShowId, title, 
                            $"Triển lãm {status.KoiShow.Name} đã kết thúc. Cảm ơn bạn đã tham gia tổ chức sự kiện.");
                        
                        // Gửi thông báo cho tất cả user khác
                        await SendToAllUsersExceptStaff(title, message, staffIds);
                        break;

                    default:
                        // Chỉ gửi thông báo khi bắt đầu KoiCheckIn
                        if (showProgress == ShowProgress.KoiCheckIn)
                        {
                            // Gửi thông báo cho staff
                            await SendToStaff(status.KoiShowId, title, 
                                $"Triển lãm {status.KoiShow.Name} đang chính thức diễn ra. Chuẩn bị đón tiếp người tham dự.");
                            
                            // Gửi thông báo cho người đã đăng ký
                            await SendToRegisteredUsers(status.KoiShowId, title, 
                                $"Triển lãm {status.KoiShow.Name} đang diễn ra. Hãy đến check-in theo lịch trình sự kiện.");
                            
                            // Gửi thông báo cho người mua vé
                            await SendToTicketPurchasers(status.KoiShowId, title, 
                                $"Triển lãm {status.KoiShow.Name} đang diễn ra. Hãy đến tham dự theo lịch trình sự kiện.");
                            
                            // Gửi thông báo cho các user còn lại
                            await SendToOtherUsers(status.KoiShowId, title, message);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo cho {ShowProgress}", showProgress);
            }
        }

        // Lấy nội dung thông báo dựa trên ShowProgress
        private string GetNotificationMessage(ShowProgress showProgress)
        {
            return showProgress switch
            {
                ShowProgress.RegistrationOpen => "Đăng ký tham gia triển lãm đã được mở!",
                ShowProgress.KoiCheckIn => "Triển lãm đang diễn ra!",
                ShowProgress.TicketCheckIn => "Triển lãm đang diễn ra!",
                ShowProgress.Preliminary => "Triển lãm đang diễn ra!",
                ShowProgress.Evaluation => "Triển lãm đang diễn ra!",
                ShowProgress.Final => "Triển lãm đang diễn ra!",
                ShowProgress.Exhibition => "Triển lãm đang diễn ra!",
                ShowProgress.PublicResult => "Triển lãm đang diễn ra!",
                ShowProgress.Award => "Triển lãm đang diễn ra!",
                ShowProgress.Finished => "Triển lãm đã kết thúc!",
                _ => "Cập nhật trạng thái triển lãm"
            };
        }

        // Lấy danh sách staff IDs
        private async Task<List<Guid>> GetShowStaffIds(Guid showId)
        {
            var staffs = await _unitOfWork.GetRepository<ShowStaff>()
                .GetListAsync(predicate: s => s.KoiShowId == showId);

            return staffs.Select(s => s.AccountId).Distinct().ToList();
        }

        // Gửi thông báo cho staff
        private async Task SendToStaff(Guid showId, string title, string message)
        {
            var staffIds = await GetShowStaffIds(showId);
            if (staffIds.Any())
            {
                await _notificationService.SendNotificationToMany(
                    staffIds, title, message, NotificationType.Show);
            }
        }

        // Gửi thông báo cho người dùng đã đăng ký
        private async Task SendToRegisteredUsers(Guid showId, string title, string message)
        {
            var registeredUsers = await _unitOfWork.GetRepository<Registration>()
                .GetListAsync(predicate: r => r.KoiShowId == showId);
            
            if (registeredUsers.Any())
            {
                await _notificationService.SendNotificationToMany(
                    registeredUsers.Select(r => r.AccountId).Distinct().ToList(),
                    title, message, NotificationType.Show);
            }
        }

        // Gửi thông báo cho người mua vé
        private async Task SendToTicketPurchasers(Guid showId, string title, string message)
        {
            var ticketTypes = await _unitOfWork.GetRepository<TicketType>()
                .GetListAsync(predicate: tt => tt.KoiShowId == showId);
                
            if (!ticketTypes.Any())
                return;
                
            var ticketTypeIds = ticketTypes.Select(tt => tt.Id).ToList();
            
            var ticketOrderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
                .GetListAsync(
                    predicate: tod => ticketTypeIds.Contains(tod.TicketTypeId),
                    include: query => query.Include(tod => tod.TicketOrder)
                );
                
            var paidTicketOrders = ticketOrderDetails
                .Select(tod => tod.TicketOrder)
                .Where(to => to.Status?.ToLower() == "paid")
                .GroupBy(to => to.AccountId)
                .Select(g => g.Key)
                .Where(id => id != null)
                .ToList();
                
            if (paidTicketOrders.Any())
            {
                await _notificationService.SendNotificationToMany(
                    paidTicketOrders, title, message, NotificationType.Show);
            }
        }

        // Gửi thông báo cho tất cả user trừ staff
        private async Task SendToAllUsersExceptStaff(string title, string message, List<Guid> staffIds)
        {
            var users = await _unitOfWork.GetRepository<Account>()
                .GetListAsync(predicate: u => !staffIds.Contains(u.Id));

            if (users.Any())
            {
                await _notificationService.SendNotificationToMany(
                    users.Select(u => u.Id).ToList(),
                    title, message, NotificationType.Show);
            }
        }

        // Gửi thông báo cho các user không phải staff và không đăng ký/mua vé
        private async Task SendToOtherUsers(Guid showId, string title, string message)
        {
            var staffIds = await GetShowStaffIds(showId);
            
            var registeredUserIds = await _unitOfWork.GetRepository<Registration>()
                .GetListAsync(predicate: r => r.KoiShowId == showId)
                .ContinueWith(t => t.Result.Select(r => r.AccountId).Distinct().ToList());
            
            var ticketTypes = await _unitOfWork.GetRepository<TicketType>()
                .GetListAsync(predicate: tt => tt.KoiShowId == showId);
                
            var ticketUserIds = new List<Guid>();
            
            if (ticketTypes.Any())
            {
                var ticketTypeIds = ticketTypes.Select(tt => tt.Id).ToList();
                var ticketOrderDetails = await _unitOfWork.GetRepository<TicketOrderDetail>()
                    .GetListAsync(
                        predicate: tod => ticketTypeIds.Contains(tod.TicketTypeId),
                        include: query => query.Include(tod => tod.TicketOrder)
                    );
                    
                ticketUserIds = ticketOrderDetails
                    .Select(tod => tod.TicketOrder)
                    .Where(to => to.Status?.ToLower() == "paid" && to.AccountId != null)
                    .Select(to => (Guid)to.AccountId)
                    .Distinct()
                    .ToList();
            }
            
            var excludeIds = staffIds
                .Union(registeredUserIds)
                .Union(ticketUserIds)
                .ToList();
            
            var users = await _unitOfWork.GetRepository<Account>()
                .GetListAsync(predicate: u => !excludeIds.Contains(u.Id));

            if (users.Any())
            {
                await _notificationService.SendNotificationToMany(
                    users.Select(u => u.Id).ToList(),
                    title, message, NotificationType.Show);
            }
        }

        // Phương thức để lên lịch các công việc Hangfire cho các trạng thái của triển lãm
        private async Task ScheduleShowStatusJobs(Guid showId)
        {
            // Lấy danh sách tất cả các trạng thái của show, sắp xếp theo thời gian bắt đầu
            var showStatuses = await _unitOfWork.GetRepository<ShowStatus>()
                .GetListAsync(
                    predicate: x => x.KoiShowId == showId,
                    include: query => query.Include(s => s.KoiShow),
                    orderBy: query => query.OrderBy(s => s.StartDate));

            if (!showStatuses.Any())
                return;

            var currentTime = VietNamTimeUtil.GetVietnamTime();
                
            // Xác định trạng thái hiện tại dựa trên thời gian
            var currentStatus = showStatuses.FirstOrDefault(s => 
                currentTime >= s.StartDate && currentTime <= s.EndDate);
                
            if (currentStatus != null)
            {
                // Kích hoạt ngay lập tức trạng thái hiện tại
                _backgroundJobClient.Enqueue(
                    () => ActivateShowStatus(showId, currentStatus.Id, currentStatus.StatusName));
                    
                _logger.LogInformation(
                    "Enqueued immediate activation for current status of show {ShowId}, status {StatusName}",
                    showId, currentStatus.StatusName);
            }
            else
            {
                // Nếu không có trạng thái hiện tại, tìm trạng thái sắp tới gần nhất
                var upcomingStatus = showStatuses.FirstOrDefault(s => s.StartDate > currentTime);
                
                if (upcomingStatus != null)
                {
                    // Lên lịch kích hoạt trạng thái sắp tới
                    _backgroundJobClient.Schedule(
                        () => ActivateShowStatus(showId, upcomingStatus.Id, upcomingStatus.StatusName),
                        upcomingStatus.StartDate - currentTime);
                        
                    _logger.LogInformation(
                        "Scheduled activation for first upcoming status of show {ShowId}, status {StatusName} at {StartDate}",
                        showId, upcomingStatus.StatusName, upcomingStatus.StartDate);
                }
                else
                {
                    // Kiểm tra nếu có trạng thái đã kết thúc gần đây nhất (trường hợp published sau khi đã có trạng thái kết thúc)
                    var latestEndedStatus = showStatuses
                        .Where(s => s.EndDate < currentTime)
                        .OrderByDescending(s => s.EndDate)
                        .FirstOrDefault();
                        
                    if (latestEndedStatus != null)
                    {
                        // Kích hoạt trạng thái đã kết thúc gần đây nhất
                        _backgroundJobClient.Enqueue(
                            () => ActivateShowStatus(showId, latestEndedStatus.Id, latestEndedStatus.StatusName));
                            
                        _logger.LogInformation(
                            "Enqueued activation for latest ended status of show {ShowId}, status {StatusName}",
                            showId, latestEndedStatus.StatusName);
                    }
                }
            }
            
            // Đối với trạng thái RegistrationOpen, vẫn lên lịch thông báo đóng đăng ký
            var registrationStatus = showStatuses.FirstOrDefault(s => 
                s.StatusName == ShowProgress.RegistrationOpen.ToString() && s.EndDate > currentTime);
                
            if (registrationStatus != null)
            {
                _backgroundJobClient.Schedule(
                    () => SendRegistrationClosedNotification(showId, registrationStatus.Id),
                    registrationStatus.EndDate - currentTime);
                    
                _logger.LogInformation(
                    "Scheduled registration closed notification for show {ShowId} at {EndDate}",
                    showId, registrationStatus.EndDate);
            }
        }
        
        // Phương thức để kích hoạt trạng thái show (chạy bởi Hangfire)
        [AutomaticRetry(Attempts = 3)]
        public async Task ActivateShowStatus(Guid showId, Guid statusId, string statusName)
        {
            var showStatus = await _unitOfWork.GetRepository<ShowStatus>()
                .SingleOrDefaultAsync(
                    predicate: x => x.Id == statusId && x.KoiShowId == showId,
                    include: query => query.Include(s => s.KoiShow));
                    
            if (showStatus == null || showStatus.IsActive)
                return;
                
            // Hủy kích hoạt trạng thái hiện tại trước khi kích hoạt trạng thái mới
            var currentActiveStatus = await _unitOfWork.GetRepository<ShowStatus>()
                .SingleOrDefaultAsync(
                    predicate: x => x.KoiShowId == showId && x.IsActive);
                    
            if (currentActiveStatus != null)
            {
                currentActiveStatus.IsActive = false;
                _unitOfWork.GetRepository<ShowStatus>().UpdateAsync(currentActiveStatus);
            }
            
            // Kích hoạt trạng thái mới
            showStatus.IsActive = true;
            await UpdateShowStatusInternal(showStatus, Enum.Parse<ShowProgress>(statusName));
            
            // Gửi thông báo
            await SendNotifications(showStatus, Enum.Parse<ShowProgress>(statusName));
            
            // Nếu đây là trạng thái "Finished", đặt cờ cho biết triển lãm đã kết thúc
            if (statusName == ShowProgress.Finished.ToString())
            {
                var show = await _unitOfWork.GetRepository<KoiShow>()
                    .SingleOrDefaultAsync(predicate: s => s.Id == showId);
                    
                if (show != null && show.Status != Domain.Enums.ShowStatus.Finished.ToString().ToLower())
                {
                    show.Status = Domain.Enums.ShowStatus.Finished.ToString().ToLower();
                    _unitOfWork.GetRepository<KoiShow>().UpdateAsync(show);
                    await _unitOfWork.CommitAsync();
                    
                    _logger.LogInformation(
                        "Show {ShowId} marked as fully finished after activating Finished status",
                        showId);
                }
            }
            
            // Lập lịch kích hoạt trạng thái tiếp theo nếu có
            // và nếu trạng thái tiếp theo có thời gian bắt đầu lớn hơn hiện tại
            var nextStatus = await _unitOfWork.GetRepository<ShowStatus>()
                .SingleOrDefaultAsync(
                    predicate: x => x.KoiShowId == showId && 
                                   x.StartDate > showStatus.StartDate,
                    orderBy: q => q.OrderBy(s => s.StartDate));
                                   
            if (nextStatus != null)
            {
                var currentTime = VietNamTimeUtil.GetVietnamTime();
                
                if (nextStatus.StartDate > currentTime)
                {
                    // Lên lịch kích hoạt trạng thái tiếp theo
                    _backgroundJobClient.Schedule(
                        () => ActivateShowStatus(showId, nextStatus.Id, nextStatus.StatusName),
                        nextStatus.StartDate - currentTime);
                        
                    _logger.LogInformation(
                        "Scheduled next status activation for show {ShowId}, status {StatusName} at {StartDate}",
                        showId, nextStatus.StatusName, nextStatus.StartDate);
                }
                else if (currentTime >= nextStatus.StartDate && currentTime <= nextStatus.EndDate)
                {
                    // Kích hoạt ngay lập tức nếu đã đến thời gian
                    _backgroundJobClient.Enqueue(
                        () => ActivateShowStatus(showId, nextStatus.Id, nextStatus.StatusName));
                        
                    _logger.LogInformation(
                        "Enqueued immediate activation for next status of show {ShowId}, status {StatusName}",
                        showId, nextStatus.StatusName);
                }
            }
            else
            {
                _logger.LogInformation(
                    "No next status found for show {ShowId} after {CurrentStatus}. This status will remain active.",
                    showId, statusName);
                    
                // Nếu không có trạng thái tiếp theo và đây không phải là trạng thái Finished,
                // có thể đây là trạng thái cuối cùng - đảm bảo rằng nó sẽ không bị hủy
                if (statusName != ShowProgress.Finished.ToString())
                {
                    _logger.LogWarning(
                        "Current status {StatusName} for show {ShowId} appears to be the final status but is not 'Finished'", 
                        statusName, showId);
                }
            }
        }
        
        // Phương thức để gửi thông báo đóng đăng ký (chạy bởi Hangfire)
        [AutomaticRetry(Attempts = 3)]
        public async Task SendRegistrationClosedNotification(Guid showId, Guid statusId)
        {
            var showStatus = await _unitOfWork.GetRepository<ShowStatus>()
                .SingleOrDefaultAsync(
                    predicate: x => x.Id == statusId && x.KoiShowId == showId,
                    include: query => query.Include(s => s.KoiShow));
                    
            if (showStatus == null)
                return;
                
            var title = $"Thông báo: {showStatus.KoiShow.Name}";
            var message = $"Đăng ký tham gia triển lãm {showStatus.KoiShow.Name} đã kết thúc.";
            
            // Thông báo cho staff
            await SendToStaff(showId, title, 
                $"Đăng ký tham gia triển lãm {showStatus.KoiShow.Name} đã kết thúc. Vui lòng xử lý các đơn đăng ký cuối cùng.");
            
            // Thông báo cho tất cả user khác
            var staffIds = await GetShowStaffIds(showId);
            await SendToAllUsersExceptStaff(title, message, staffIds);
        }

        // Phương thức để hủy tất cả công việc đã lên lịch cho một show
        [AutomaticRetry(Attempts = 3)]
        public async Task CancelScheduledShowJobs(Guid showId)
        {
            // Lưu ý: Hangfire không có API trực tiếp để hủy công việc dựa trên tham số
            // Giải pháp thay thế là đánh dấu tất cả trạng thái của show là không hoạt động
            var showStatuses = await _unitOfWork.GetRepository<ShowStatus>()
                .GetListAsync(
                    predicate: x => x.KoiShowId == showId,
                    include: query => query.Include(s => s.KoiShow));
                    
            foreach (var status in showStatuses)
            {
                if (status.IsActive)
                {
                    status.IsActive = false;
                    _unitOfWork.GetRepository<ShowStatus>().UpdateAsync(status);
                }
            }
            
            await _unitOfWork.CommitAsync();
            _logger.LogInformation(
                "Marked all statuses as inactive for cancelled show {ShowId}",
                showId);
                
            // Gửi thông báo hủy triển lãm đến tất cả người dùng
            var show = await _unitOfWork.GetRepository<KoiShow>()
                .SingleOrDefaultAsync(predicate: s => s.Id == showId);
                
            if (show != null)
            {
                var title = $"Thông báo: {show.Name}";
                var message = $"Triển lãm {show.Name} đã bị hủy.";
                
                // Không cần phân biệt người dùng khi triển lãm bị hủy, gửi thông báo chung
                var allUsers = await _unitOfWork.GetRepository<Account>()
                    .GetListAsync(selector: a => a.Id);
                    
                if (allUsers.Any())
                {
                    await _notificationService.SendNotificationToMany(
                        allUsers.ToList(),
                        title,
                        message,
                        NotificationType.Show);
                }
            }
        }
    }
}