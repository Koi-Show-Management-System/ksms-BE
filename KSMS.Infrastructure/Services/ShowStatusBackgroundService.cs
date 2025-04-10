using KSMS.Application.Repositories;
using KSMS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KSMS.Application.Services;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;
using ShowStatus = KSMS.Domain.Entities.ShowStatus;

namespace KSMS.Infrastructure.Services
{
    public class ShowStatusBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ShowStatusBackgroundService> _logger;
        private readonly Dictionary<ShowProgress, string> _notificationMessages;
        private readonly IMemoryCache _cache;
        private const string NOTIFICATION_CACHE_KEY = "ShowStatus_{0}_{1}"; // ShowId_StatusName

        public ShowStatusBackgroundService(
            IServiceScopeFactory scopeFactory, 
            ILogger<ShowStatusBackgroundService> logger,
            IMemoryCache cache)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _cache = cache;
            _notificationMessages = new Dictionary<ShowProgress, string>
            {
                { ShowProgress.RegistrationOpen, "Đăng ký tham gia triển lãm đã được mở!" },
                { ShowProgress.RegistrationClosed, "Đăng ký tham gia triển lãm đã đóng." },
                { ShowProgress.CheckIn, "Chuẩn bị bắt đầu check-in." },
                { ShowProgress.Preliminary, "Vòng sơ loại đã bắt đầu!" },
                { ShowProgress.Evaluation, "Vòng đánh giá đã bắt đầu!" },
                { ShowProgress.Final, "Vòng chung kết đã bắt đầu!" },
                // { ShowProgress.GrandChampion, "Vòng Grand Champion đã bắt đầu!" },
                // { ShowProgress.Completed, "Triển lãm đã hoàn thành!" },
                { ShowProgress.Exhibition, "Triển lãm đang diễn ra!" },
                { ShowProgress.Finished, "Triển lãm đã kết thúc!" }
            };
        }

        private bool HasNotificationBeenSent(Guid showId, string statusName)
        {
            var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
            return _cache.TryGetValue(cacheKey, out _);
        }

        private void MarkNotificationAsSent(Guid showId, string statusName)
        {
            var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
            _cache.Set(cacheKey, true, TimeSpan.FromHours(24));
        }
        private void ClearNotificationCache(Guid showId, string statusName)
        {
            var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
            _cache.Remove(cacheKey);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    var currentTime = VietNamTimeUtil.GetVietnamTime();
                    //var fifteenMinutesLater = currentTime.AddMinutes(15);

                    var showStatuses = await unitOfWork.GetRepository<ShowStatus>()
                        .GetListAsync(predicate: x => x.KoiShow.Status != Domain.Enums.ShowStatus.Cancelled.ToString() &&
                                                      x.KoiShow.Status != Domain.Enums.ShowStatus.Finished.ToString() &&
                                                        x.KoiShow.Status != Domain.Enums.ShowStatus.Pending.ToString() &&
                                                      x.KoiShow.Status != Domain.Enums.ShowStatus.InternalPublished.ToString()
                            ,include: query => query.Include(s => s.KoiShow));

                    foreach (var status in showStatuses)
                    {
                        var isNowActive = currentTime >= status.StartDate && currentTime <= status.EndDate;
                        //var willBeActiveSoon = fifteenMinutesLater >= status.StartDate && fifteenMinutesLater <= status.EndDate;
                        var showProgress = Enum.Parse<ShowProgress>(status.StatusName);
                        var nextStatus = showStatuses.Where(
                            s => s.KoiShowId == status.KoiShowId &&
                                 s.StartDate > status.StartDate).MinBy(s => s.StartDate);
                        var shouldBeActive = isNowActive ||
                                             (status.IsActive && nextStatus != null && currentTime < nextStatus.StartDate);
                        if (status.IsActive != shouldBeActive)
                        {
                            status.IsActive = isNowActive;
                            await UpdateShowStatus(unitOfWork, status, showProgress);

                            // if (isNowActive)
                            // {
                            //     ClearNotificationCache(status.KoiShowId, status.StatusName);
                            //     if (!HasNotificationBeenSent(status.KoiShowId, status.StatusName))
                            //     {
                            //         await SendNotifications(notificationService, status, showProgress);
                            //         MarkNotificationAsSent(status.KoiShowId, status.StatusName);
                            //     }
                            // }
                        }
                        // else if (!status.IsActive && willBeActiveSoon && !HasNotificationBeenSent(status.KoiShowId, status.StatusName))
                        // {
                        //     await SendUpcomingNotifications(notificationService, status, showProgress);
                        //     MarkNotificationAsSent(status.KoiShowId, status.StatusName);
                        // }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong ShowStatusBackgroundService");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task UpdateShowStatus(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ShowStatus status, ShowProgress showProgress)
        {
            try
            {
                var showStatusRepo = unitOfWork.GetRepository<ShowStatus>();
                var koiShowRepo = unitOfWork.GetRepository<KoiShow>();

                showStatusRepo.UpdateAsync(status);

                if (status.IsActive)
                {
                    var newStatus = showProgress switch
                    {
                        ShowProgress.RegistrationOpen or 
                        ShowProgress.RegistrationClosed or 
                        ShowProgress.CheckIn => "upcoming",
                        
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

                await unitOfWork.CommitAsync();
                _logger.LogInformation(
                    "Updated status for show {ShowId}: IsActive={IsActive}, Status={Status}", 
                    status.KoiShowId, 
                    status.IsActive, 
                    status.KoiShow.Status
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error updating status for show {ShowId}", 
                    status.KoiShowId
                );
                throw;
            }
        }

        private async Task SendNotifications(INotificationService notificationService, ShowStatus status, ShowProgress showProgress)
        {
            try
            {
                var message = _notificationMessages[showProgress];
                var title = $"Thông báo: {status.KoiShow.Name}";
                var staffIds = await GetShowStaffIds(status.KoiShowId);
                var refereeIds = await GetRefereeIds(status.KoiShowId, showProgress);

                switch (showProgress)
                {
                    case ShowProgress.RegistrationOpen:
                        // Gửi thông báo cho staff với nội dung riêng
                        await SendToShowStaff(notificationService, status.KoiShowId, title, 
                            $"Triển lãm {status.KoiShow.Name} đã mở đăng ký. Vui lòng theo dõi và xử lý các đơn đăng ký.");
                        
                        // Gửi thông báo cho tất cả user khác (không bao gồm staff và referee)
                        await SendToAllUsersExceptStaffAndReferees(notificationService, title, message, staffIds, refereeIds);
                        break;

                    case ShowProgress.Preliminary:
                    case ShowProgress.Evaluation:
                    case ShowProgress.Final:
                        // Gửi thông báo cho từng nhóm với nội dung riêng
                        await SendToReferees(notificationService, status.KoiShow, showProgress, title, message, isUpcoming: false);
                        await SendToShowStaff(notificationService, status.KoiShowId, title, message);
                        await SendToAllUsersExceptStaffAndReferees(notificationService, title, message, staffIds, refereeIds);
                        break;

                    case ShowProgress.CheckIn:
                        await SendToConfirmedRegistrations(notificationService, status.KoiShowId, title, message);
                        break;

                    case ShowProgress.RegistrationClosed:
                        await SendToRegisteredUsers(notificationService, status.KoiShowId, title, message);
                        break;

                    default:
                        // Gửi thông báo cho từng nhóm với cùng nội dung
                        await SendToShowStaff(notificationService, status.KoiShowId, title, message);
                        await SendToAllUsersExceptStaffAndReferees(notificationService, title, message, staffIds, refereeIds);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo cho {ShowProgress}", showProgress);
            }
        }

        private async Task SendUpcomingNotifications(INotificationService notificationService, ShowStatus status, ShowProgress showProgress)
        {
            try 
            {
                var message = $"Sắp bắt đầu: {_notificationMessages[showProgress]}";
                var title = $"Thông báo: {status.KoiShow.Name}";
                var staffIds = await GetShowStaffIds(status.KoiShowId);
                var refereeIds = await GetRefereeIds(status.KoiShowId, showProgress);

                switch (showProgress)
                {
                    case ShowProgress.Preliminary:
                    case ShowProgress.Evaluation:
                    case ShowProgress.Final:
                        await SendToReferees(notificationService, status.KoiShow, showProgress, title, message, isUpcoming: true);
                        break;
                    
                    case ShowProgress.RegistrationOpen:
                        // Gửi thông báo cho staff với nội dung riêng
                        await SendToShowStaff(notificationService, status.KoiShowId, title, 
                            $"Sắp bắt đầu: Triển lãm {status.KoiShow.Name} sẽ mở đăng ký. Vui lòng chuẩn bị theo dõi và xử lý các đơn đăng ký.");
                        
                        // Gửi thông báo cho tất cả user khác
                        await SendToAllUsersExceptStaffAndReferees(notificationService, title, message, staffIds, refereeIds);
                        break;

                    default:
                        // Gửi thông báo cho staff
                        await SendToShowStaff(notificationService, status.KoiShowId, title, message);
                        // Gửi thông báo cho user khác
                        await SendToAllUsersExceptStaffAndReferees(notificationService, title, message, staffIds, refereeIds);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi thông báo sắp bắt đầu cho {ShowProgress}", showProgress);
            }
        }

        private async Task SendToAllUsers(INotificationService notificationService, string title, string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
            
            var users = await unitOfWork.GetRepository<Account>()
                .GetListAsync();
            
            await notificationService.SendNotificationToMany(
                users.Select(u => u.Id).ToList(),
                title,
                message,
                NotificationType.Show
            );
        }

        private async Task SendToConfirmedRegistrations(INotificationService notificationService, Guid showId, string title, string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
            
            var confirmedUsers = await unitOfWork.GetRepository<Registration>()
                .GetListAsync(
                    predicate: r => r.KoiShowId == showId && 
                                   r.Status == RegistrationStatus.Confirmed.ToString().ToLower()
                );
            
            if (confirmedUsers.Any())
            {
                await notificationService.SendNotificationToMany(
                    confirmedUsers.Select(r => r.AccountId).Distinct().ToList(),
                    title,
                    message,
                    NotificationType.Show
                );
            }
        }

        private async Task SendToRegisteredUsers(INotificationService notificationService, Guid showId, string title, string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
            
            var registeredUsers = await unitOfWork.GetRepository<Registration>()
                .GetListAsync(predicate: r => r.KoiShowId == showId);
            
            if (registeredUsers.Any())
            {
                await notificationService.SendNotificationToMany(
                    registeredUsers.Select(r => r.AccountId).Distinct().ToList(),
                    title,
                    message,
                    NotificationType.Show
                );
            }
        }

        private async Task SendToReferees(
            INotificationService notificationService, 
            KoiShow koiShow,
            ShowProgress currentRound,
            string title, 
            string message,
            bool isUpcoming)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();

            var categories = await unitOfWork.GetRepository<CompetitionCategory>()
                .GetListAsync(predicate: c => c.KoiShowId == koiShow.Id);

            if (!categories.Any())
            {
                _logger.LogWarning("No categories found for show {ShowId}", koiShow.Id);
                return;
            }

            var roundType = currentRound.ToString();
            var refereeAssignments = await unitOfWork.GetRepository<RefereeAssignment>()
                .GetListAsync(
                    predicate: r => 
                        categories.Select(c => c.Id).Contains(r.CompetitionCategoryId) && 
                        r.RoundType == roundType,
                    include: query => query
                        .Include(r => r.RefereeAccount)
                        .Include(r => r.CompetitionCategory)
                );

            if (!refereeAssignments.Any())
            {
                _logger.LogWarning(
                    "No referee assignments found for show {ShowId} in round {Round}", 
                    koiShow.Id, 
                    roundType
                );
                return;
            }

            var refereeGroups = refereeAssignments
                .GroupBy(r => r.RefereeAccountId)
                .ToList();

            foreach (var group in refereeGroups)
            {
                var refereeId = group.Key;
                var categoriess = group.Select(r => r.CompetitionCategory.Name).ToList();
                
                var refereeMessage = isUpcoming
                    ? $"Thông báo: Trong 15 phút nữa bạn sẽ bắt đầu chấm điểm cho vòng {roundType} tại triển lãm {koiShow.Name}.\n" +
                      $"Các hạng mục bạn phụ trách: {string.Join(", ", categoriess)}"
                    : $"Vòng {roundType} đã bắt đầu tại triển lãm {koiShow.Name}.\n" +
                      $"Bạn được phân công chấm điểm cho các hạng mục: {string.Join(", ", categoriess)}";

                await notificationService.SendNotification(
                    refereeId,
                    title,
                    refereeMessage,
                    NotificationType.Show
                );

                _logger.LogInformation(
                    "Sent {NotificationType} notification to referee {RefereeId} for {CategoryCount} categories in show {ShowName} round {Round}", 
                    isUpcoming ? "upcoming" : "start",
                    refereeId,
                    categories.Count,
                    koiShow.Name,
                    roundType
                );
            }
        }

        private async Task<List<Guid>> GetRefereeIds(Guid showId, ShowProgress currentRound)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();

            var categories = await unitOfWork.GetRepository<CompetitionCategory>()
                .GetListAsync(predicate: c => c.KoiShowId == showId);

            if (!categories.Any())
                return new List<Guid>();

            var roundType = currentRound.ToString();
            var refereeAssignments = await unitOfWork.GetRepository<RefereeAssignment>()
                .GetListAsync(
                    predicate: r => 
                        categories.Select(c => c.Id).Contains(r.CompetitionCategoryId) && 
                        r.RoundType == roundType
                );

            return refereeAssignments
                .Select(r => r.RefereeAccountId)
                .Distinct()
                .ToList();
        }
        private async Task<List<Guid>> GetShowStaffIds(Guid showId)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();

            var staffs = await unitOfWork.GetRepository<ShowStaff>()
                .GetListAsync(predicate: s => s.KoiShowId == showId);

            return staffs.Select(s => s.AccountId).Distinct().ToList();
        }

        private async Task SendToShowStaff(
            INotificationService notificationService,
            Guid showId,
            string title,
            string message)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();

            var staffs = await unitOfWork.GetRepository<ShowStaff>()
                .GetListAsync(predicate: s => s.KoiShowId == showId);

            if (staffs.Any())
            {
                await notificationService.SendNotificationToMany(
                    staffs.Select(s => s.AccountId).Distinct().ToList(),
                    title,
                    message,
                    NotificationType.Show
                );
            }
        }

        private async Task SendToAllUsersExceptStaffAndReferees(
            INotificationService notificationService,
            string title,
            string message,
            List<Guid> staffIds,
            List<Guid> refereeIds)
        {
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();

            var excludeIds = staffIds.Union(refereeIds).ToList();
            
            var users = await unitOfWork.GetRepository<Account>()
                .GetListAsync(
                    predicate: u => !excludeIds.Contains(u.Id)
                );

            if (users.Any())
            {
                await notificationService.SendNotificationToMany(
                    users.Select(u => u.Id).ToList(),
                    title,
                    message,
                    NotificationType.Show
                );
            }
        }
    }
}