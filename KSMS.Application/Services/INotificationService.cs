using KSMS.Domain.Dtos.Responses.Notification;
using KSMS.Domain.Enums;
using KSMS.Domain.Pagination;

namespace KSMS.Application.Services;

public interface INotificationService
{
    Task SendNotification(Guid accountId, string title, string message, NotificationType type);
    Task SendNotificationToMany(List<Guid> accountIds, string title, string message, NotificationType type);
    
    Task MarkNotificationAsRead(Guid notificationId);
    Task MarkAllNotificationAsRead(Guid accountId);
    
    Task<Paginate<GetPageNotificationResponse>> GetPageNotification(Guid accountId, bool? isRead, NotificationType? notificationType, int page, int size);
} 