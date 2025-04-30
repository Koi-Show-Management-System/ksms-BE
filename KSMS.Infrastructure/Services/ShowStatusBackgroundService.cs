// using KSMS.Application.Repositories;
// using KSMS.Domain.Entities;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using Microsoft.Extensions.Caching.Memory;
//
// using System;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using KSMS.Application.Services;
// using KSMS.Domain.Enums;
// using KSMS.Infrastructure.Database;
// using KSMS.Infrastructure.Utils;
// using Microsoft.EntityFrameworkCore;
// using ShowStatus = KSMS.Domain.Entities.ShowStatus;
//
// namespace KSMS.Infrastructure.Services
// {
//     public class ShowStatusBackgroundService : BackgroundService
//     {
//         private readonly IServiceScopeFactory _scopeFactory;
//         private readonly ILogger<ShowStatusBackgroundService> _logger;
//         private readonly Dictionary<ShowProgress, string> _notificationMessages;
//         private readonly IMemoryCache _cache;
//         private const string NOTIFICATION_CACHE_KEY = "ShowStatus_{0}_{1}"; // ShowId_StatusName
//
//         public ShowStatusBackgroundService(
//             IServiceScopeFactory scopeFactory, 
//             ILogger<ShowStatusBackgroundService> logger,
//             IMemoryCache cache)
//         {
//             _scopeFactory = scopeFactory;
//             _logger = logger;
//             _cache = cache;
//             _notificationMessages = new Dictionary<ShowProgress, string>
//             {
//                 { ShowProgress.RegistrationOpen, "Đăng ký tham gia triển lãm đã được mở!" },
//                 { ShowProgress.KoiCheckIn, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.TicketCheckIn, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Preliminary, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Evaluation, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Final, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Exhibition, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.PublicResult, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Award, "Triển lãm đang diễn ra!" },
//                 { ShowProgress.Finished, "Triển lãm đã kết thúc!" }
//             };
//         }
//
//         private bool HasNotificationBeenSent(Guid showId, string statusName)
//         {
//             var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
//             return _cache.TryGetValue(cacheKey, out _);
//         }
//
//         private void MarkNotificationAsSent(Guid showId, string statusName)
//         {
//             var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
//             _cache.Set(cacheKey, true, TimeSpan.FromHours(24));
//         }
//         
//         private void ClearNotificationCache(Guid showId, string statusName)
//         {
//             var cacheKey = string.Format(NOTIFICATION_CACHE_KEY, showId, statusName);
//             _cache.Remove(cacheKey);
//         }
//
//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     using var scope = _scopeFactory.CreateScope();
//                     var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//                     var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
//
//                     var currentTime = VietNamTimeUtil.GetVietnamTime();
//                     //var fifteenMinutesLater = currentTime.AddMinutes(15);
//
//                     var showStatuses = await unitOfWork.GetRepository<ShowStatus>()
//                         .GetListAsync(predicate: x => x.KoiShow.Status != Domain.Enums.ShowStatus.Cancelled.ToString() &&
//                                                       x.KoiShow.Status != Domain.Enums.ShowStatus.Finished.ToString() &&
//                                                         x.KoiShow.Status != Domain.Enums.ShowStatus.Pending.ToString() &&
//                                                       x.KoiShow.Status != Domain.Enums.ShowStatus.InternalPublished.ToString()
//                             ,include: query => query.Include(s => s.KoiShow));
//
//                     foreach (var status in showStatuses)
//                     {
//                         var isNowActive = currentTime >= status.StartDate && currentTime <= status.EndDate;
//                         //var willBeActiveSoon = fifteenMinutesLater >= status.StartDate && fifteenMinutesLater <= status.EndDate;
//                         var showProgress = Enum.Parse<ShowProgress>(status.StatusName);
//                         var nextStatus = showStatuses.Where(
//                             s => s.KoiShowId == status.KoiShowId &&
//                                  s.StartDate > status.StartDate).MinBy(s => s.StartDate);
//                         var shouldBeActive = isNowActive ||
//                                              (status.IsActive && nextStatus != null && currentTime < nextStatus.StartDate);
//                         if (status.IsActive != shouldBeActive)
//                         {
//                             status.IsActive = isNowActive;
//                             await UpdateShowStatus(unitOfWork, status, showProgress);
//                             if (isNowActive)
//                             {
//                                 ClearNotificationCache(status.KoiShowId, status.StatusName);
//                                 if (!HasNotificationBeenSent(status.KoiShowId, status.StatusName))
//                                 {
//                                     await SendNotifications(notificationService, status, showProgress);
//                                     MarkNotificationAsSent(status.KoiShowId, status.StatusName);
//                                 }
//                             }
//                         }
//                         if (showProgress == ShowProgress.RegistrationOpen && currentTime > status.EndDate)
//                         {
//                             var registrationClosedKey = $"RegistrationClosed_{status.KoiShowId}";
//                             if (!_cache.TryGetValue(registrationClosedKey, out _))
//                             {
//                                 await SendRegistrationClosedNotification(notificationService, status);
//                                 _cache.Set(registrationClosedKey, true, TimeSpan.FromHours(24));
//                             }
//                         }
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError(ex, "Lỗi trong ShowStatusBackgroundService");
//                 }
//
//                 await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
//             }
//         }
//
//         private async Task UpdateShowStatus(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ShowStatus status, ShowProgress showProgress)
//         {
//             try
//             {
//                 var showStatusRepo = unitOfWork.GetRepository<ShowStatus>();
//                 var koiShowRepo = unitOfWork.GetRepository<KoiShow>();
//
//                 showStatusRepo.UpdateAsync(status);
//
//                 if (status.IsActive)
//                 {
//                     var newStatus = showProgress switch
//                     {
//                         ShowProgress.RegistrationOpen or 
//                         ShowProgress.KoiCheckIn or
//                         ShowProgress.TicketCheckIn => "upcoming",
//                         
//                         ShowProgress.Preliminary or 
//                         ShowProgress.Evaluation or 
//                         ShowProgress.Final or 
//                         ShowProgress.Award or 
//                         ShowProgress.PublicResult or 
//                         ShowProgress.Exhibition => "inprogress",
//                         
//                         ShowProgress.Finished => "finished",
//                         _ => status.KoiShow.Status
//                     };
//
//                     if (status.KoiShow.Status != newStatus)
//                     {
//                         status.KoiShow.Status = newStatus;
//                         koiShowRepo.UpdateAsync(status.KoiShow);
//                     }
//                 }
//
//                 await unitOfWork.CommitAsync();
//                 _logger.LogInformation(
//                     "Updated status for show {ShowId}: IsActive={IsActive}, Status={Status}", 
//                     status.KoiShowId, 
//                     status.IsActive, 
//                     status.KoiShow.Status
//                 );
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, 
//                     "Error updating status for show {ShowId}", 
//                     status.KoiShowId
//                 );
//                 throw;
//             }
//         }
//
//         private async Task SendRegistrationClosedNotification(INotificationService notificationService, ShowStatus status)
//         {
//             try
//             {
//                 var title = $"Thông báo: {status.KoiShow.Name}";
//                 var message = $"Đăng ký tham gia triển lãm {status.KoiShow.Name} đã kết thúc.";
//                 
//                 var staffIds = await GetShowStaffIds(status.KoiShowId);
//                 
//                 // Thông báo cho staff
//                 await SendToShowStaff(notificationService, status.KoiShowId, title, 
//                     $"Đăng ký tham gia triển lãm {status.KoiShow.Name} đã kết thúc. Vui lòng xử lý các đơn đăng ký cuối cùng.");
//                 
//                 // Thông báo cho tất cả user khác
//                 await SendToAllUsersExceptStaff(notificationService, title, message, staffIds);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Lỗi khi gửi thông báo đóng đăng ký cho show {ShowId}", status.KoiShowId);
//             }
//         }
//
//         private async Task SendNotifications(INotificationService notificationService, ShowStatus status, ShowProgress showProgress)
//         {
//             try
//             {
//                 var message = _notificationMessages[showProgress];
//                 var title = $"Thông báo: {status.KoiShow.Name}";
//                 var staffIds = await GetShowStaffIds(status.KoiShowId);
//
//                 switch (showProgress)
//                 {
//                     case ShowProgress.RegistrationOpen:
//                         // Gửi thông báo cho staff với nội dung riêng
//                         await SendToShowStaff(notificationService, status.KoiShowId, title, 
//                             $"Triển lãm {status.KoiShow.Name} đã mở đăng ký. Vui lòng theo dõi và xử lý các đơn đăng ký.");
//                         
//                         // Gửi thông báo cho tất cả user khác
//                         await SendToAllUsersExceptStaff(notificationService, title, message, staffIds);
//                         break;
//
//                     case ShowProgress.Finished:
//                         // Gửi thông báo kết thúc triển lãm
//                         await SendToShowStaff(notificationService, status.KoiShowId, title, 
//                             $"Triển lãm {status.KoiShow.Name} đã kết thúc. Cảm ơn bạn đã tham gia tổ chức sự kiện.");
//                         
//                         // Gửi thông báo cho tất cả user khác
//                         await SendToAllUsersExceptStaff(notificationService, title, message, staffIds);
//                         break;
//
//                     default:
//                         // Chỉ gửi thông báo triển lãm đang diễn ra 1 lần duy nhất (khi bắt đầu KoiCheckIn)
//                         if (showProgress == ShowProgress.KoiCheckIn)
//                         {
//                             // Gửi thông báo cho staff
//                             await SendToShowStaff(notificationService, status.KoiShowId, title, 
//                                 $"Triển lãm {status.KoiShow.Name} đang chính thức diễn ra. Chuẩn bị đón tiếp người tham dự.");
//                             
//                             // Gửi thông báo cho người đã đăng ký
//                             await SendToRegisteredUsers(notificationService, status.KoiShowId, title, 
//                                 $"Triển lãm {status.KoiShow.Name} đang diễn ra. Hãy đến check-in theo lịch trình sự kiện.");
//                             
//                             // Gửi thông báo cho người mua vé
//                             await SendToTicketPurchasers(notificationService, status.KoiShowId, title, 
//                                 $"Triển lãm {status.KoiShow.Name} đang diễn ra. Hãy đến tham dự theo lịch trình sự kiện.");
//                             
//                             // Gửi thông báo cho các user còn lại
//                             await SendToAllUsersExceptStaffAndParticipants(notificationService, status.KoiShowId, title, message);
//                         }
//                         break;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Lỗi khi gửi thông báo cho {ShowProgress}", showProgress);
//             }
//         }
//
//         private async Task SendToRegisteredUsers(INotificationService notificationService, Guid showId, string title, string message)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//             
//             var registeredUsers = await unitOfWork.GetRepository<Registration>()
//                 .GetListAsync(predicate: r => r.KoiShowId == showId);
//             
//             if (registeredUsers.Any())
//             {
//                 await notificationService.SendNotificationToMany(
//                     registeredUsers.Select(r => r.AccountId).Distinct().ToList(),
//                     title,
//                     message,
//                     NotificationType.Show
//                 );
//             }
//         }
//         
//         private async Task SendToTicketPurchasers(INotificationService notificationService, Guid showId, string title, string message)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//             
//             // Lấy danh sách loại vé của show
//             var ticketTypes = await unitOfWork.GetRepository<TicketType>()
//                 .GetListAsync(predicate: tt => tt.KoiShowId == showId);
//                 
//             if (!ticketTypes.Any())
//                 return;
//                 
//             var ticketTypeIds = ticketTypes.Select(tt => tt.Id).ToList();
//             
//             // Lấy danh sách chi tiết đơn hàng vé liên quan đến show
//             var ticketOrderDetails = await unitOfWork.GetRepository<TicketOrderDetail>()
//                 .GetListAsync(
//                     predicate: tod => ticketTypeIds.Contains(tod.TicketTypeId),
//                     include: query => query.Include(tod => tod.TicketOrder)
//                 );
//                 
//             // Lấy danh sách đơn hàng vé đã thanh toán
//             var paidTicketOrders = ticketOrderDetails
//                 .Select(tod => tod.TicketOrder)
//                 .Where(to => to.Status?.ToLower() == "paid")
//                 .GroupBy(to => to.AccountId) // Nhóm theo ID tài khoản để loại bỏ trùng lặp
//                 .Select(g => g.Key) // Lấy ID tài khoản
//                 .Where(id => id != null) // Loại bỏ các đơn hàng không có ID tài khoản
//                 .ToList();
//                 
//             if (paidTicketOrders.Any())
//             {
//                 await notificationService.SendNotificationToMany(
//                     paidTicketOrders,
//                     title,
//                     message,
//                     NotificationType.Show
//                 );
//             }
//         }
//
//         private async Task<List<Guid>> GetShowStaffIds(Guid showId)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//
//             var staffs = await unitOfWork.GetRepository<ShowStaff>()
//                 .GetListAsync(predicate: s => s.KoiShowId == showId);
//
//             return staffs.Select(s => s.AccountId).Distinct().ToList();
//         }
//
//         private async Task SendToShowStaff(
//             INotificationService notificationService,
//             Guid showId,
//             string title,
//             string message)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//
//             var staffs = await unitOfWork.GetRepository<ShowStaff>()
//                 .GetListAsync(predicate: s => s.KoiShowId == showId);
//
//             if (staffs.Any())
//             {
//                 await notificationService.SendNotificationToMany(
//                     staffs.Select(s => s.AccountId).Distinct().ToList(),
//                     title,
//                     message,
//                     NotificationType.Show
//                 );
//             }
//         }
//
//         private async Task SendToAllUsersExceptStaff(
//             INotificationService notificationService,
//             string title,
//             string message,
//             List<Guid> staffIds)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//             
//             var users = await unitOfWork.GetRepository<Account>()
//                 .GetListAsync(
//                     predicate: u => !staffIds.Contains(u.Id)
//                 );
//
//             if (users.Any())
//             {
//                 await notificationService.SendNotificationToMany(
//                     users.Select(u => u.Id).ToList(),
//                     title,
//                     message,
//                     NotificationType.Show
//                 );
//             }
//         }
//         
//         private async Task SendToAllUsersExceptStaffAndParticipants(
//             INotificationService notificationService,
//             Guid showId,
//             string title,
//             string message)
//         {
//             using var scope = _scopeFactory.CreateScope();
//             var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<KoiShowManagementSystemContext>>();
//             
//             // Lấy danh sách ID staff
//             var staffIds = await GetShowStaffIds(showId);
//             
//             // Lấy danh sách ID người đăng ký
//             var registeredUserIds = await unitOfWork.GetRepository<Registration>()
//                 .GetListAsync(predicate: r => r.KoiShowId == showId)
//                 .ContinueWith(t => t.Result.Select(r => r.AccountId).Distinct().ToList());
//             
//             // Lấy danh sách ID người mua vé
//             var ticketTypes = await unitOfWork.GetRepository<TicketType>()
//                 .GetListAsync(predicate: tt => tt.KoiShowId == showId);
//                 
//             var ticketUserIds = new List<Guid>();
//             
//             if (ticketTypes.Any())
//             {
//                 var ticketTypeIds = ticketTypes.Select(tt => tt.Id).ToList();
//                 var ticketOrderDetails = await unitOfWork.GetRepository<TicketOrderDetail>()
//                     .GetListAsync(
//                         predicate: tod => ticketTypeIds.Contains(tod.TicketTypeId),
//                         include: query => query.Include(tod => tod.TicketOrder)
//                     );
//                     
//                 ticketUserIds = ticketOrderDetails
//                     .Select(tod => tod.TicketOrder)
//                     .Where(to => to.Status?.ToLower() == "paid" && to.AccountId != null)
//                     .Select(to => (Guid)to.AccountId)
//                     .Distinct()
//                     .ToList();
//             }
//             
//             // Hợp nhất các danh sách ID để loại trừ
//             var excludeIds = staffIds
//                 .Union(registeredUserIds)
//                 .Union(ticketUserIds)
//                 .ToList();
//             
//             // Lấy danh sách người dùng không thuộc các nhóm trên
//             var users = await unitOfWork.GetRepository<Account>()
//                 .GetListAsync(
//                     predicate: u => !excludeIds.Contains(u.Id)
//                 );
//
//             if (users.Any())
//             {
//                 await notificationService.SendNotificationToMany(
//                     users.Select(u => u.Id).ToList(),
//                     title,
//                     message,
//                     NotificationType.Show
//                 );
//             }
//         }
//     }
// }