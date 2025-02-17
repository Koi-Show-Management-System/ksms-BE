using KSMS.Domain.Enums;

namespace KSMS.Application.Services;

public interface INotificationService
{
    Task SendNotification(Guid userId, string title, string message, NotificationType type);
    Task SendNotificationToMany(IEnumerable<Guid> userIds, string title, string message, NotificationType type);
} 