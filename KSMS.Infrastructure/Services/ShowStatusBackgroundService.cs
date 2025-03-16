using KSMS.Application.Repositories;
using KSMS.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
 
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

        public ShowStatusBackgroundService(IServiceScopeFactory scopeFactory, ILogger<ShowStatusBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _notificationMessages = new Dictionary<ShowProgress, string>
            {
                { ShowProgress.RegistrationOpen, "Đăng ký tham gia triển lãm đã được mở!" },
                { ShowProgress.RegistrationClosed, "Đăng ký tham gia triển lãm đã đóng." },
                { ShowProgress.CheckIn, "Chuẩn bị bắt đầu check-in." },
                { ShowProgress.Preliminary, "Vòng sơ loại đã bắt đầu!" },
                { ShowProgress.Evaluation, "Vòng đánh giá đã bắt đầu!" },
                { ShowProgress.Final, "Vòng chung kết đã bắt đầu!" },
                { ShowProgress.GrandChampion, "Vòng Grand Champion đã bắt đầu!" },
                { ShowProgress.Completed, "Triển lãm đã hoàn thành!" },
                { ShowProgress.Exhibition, "Triển lãm đang diễn ra!" },
                { ShowProgress.Finished, "Triển lãm đã kết thúc!" }
            };
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
                    var fifteenMinutesLater = currentTime.AddMinutes(15);

                    var showStatuses = await unitOfWork.GetRepository<ShowStatus>()
                        .GetListAsync(include: query => query
                            .Include(s => s.KoiShow));

                    foreach (var status in showStatuses)
                    {
                        var isNowActive = currentTime >= status.StartDate && currentTime <= status.EndDate;
                        var willBeActiveSoon = fifteenMinutesLater >= status.StartDate && fifteenMinutesLater <= status.EndDate;
                        var showProgress = Enum.Parse<ShowProgress>(status.StatusName);

                        if (status.IsActive != isNowActive)
                        {
                            status.IsActive = isNowActive;
                            await UpdateShowStatus(unitOfWork, status, showProgress);

                            if (isNowActive)
                            {
                                await SendNotifications(notificationService, status, showProgress);
                            }
                        }
                        else if (!status.IsActive && willBeActiveSoon)
                        {
                            await SendUpcomingNotifications(notificationService, status, showProgress);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi trong ShowStatusBackgroundService");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
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
                        ShowProgress.GrandChampion or 
                        ShowProgress.Completed or 
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

                switch (showProgress)
                {
                    case ShowProgress.RegistrationOpen:
                        await SendToAllUsers(notificationService, title, message);
                        break;

                    case ShowProgress.CheckIn:
                        await SendToConfirmedRegistrations(notificationService, status.KoiShowId, title, message);
                        break;

                    case ShowProgress.RegistrationClosed:
                        await SendToRegisteredUsers(notificationService, status.KoiShowId, title, message);
                        break;

                    case ShowProgress.Preliminary:
                    case ShowProgress.Evaluation:
                    case ShowProgress.Final:
                        await SendToReferees(notificationService, status.KoiShow, showProgress, title, message, isUpcoming: false);
                        await SendToAllUsers(notificationService, title, message);
                        break;

                    default:
                        await SendToAllUsers(notificationService, title, message);
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

                switch (showProgress)
                {
                    case ShowProgress.Preliminary:
                    case ShowProgress.Evaluation:
                    case ShowProgress.Final:
                        await SendToReferees(notificationService, status.KoiShow, showProgress, title, message, isUpcoming: true);
                        break;
                    
                    default:
                        await SendNotifications(notificationService, status, showProgress);
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
    }
}
