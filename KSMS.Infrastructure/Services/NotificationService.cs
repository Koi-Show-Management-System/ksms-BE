using KSMS.Application.Services;
using KSMS.Domain.Enums;
using KSMS.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace KSMS.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotification(Guid userId, string title, string message, NotificationType type)
    {
        var notification = new
        {
            Title = title,
            Message = message,
            Type = type.ToString(),
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToMany(IEnumerable<Guid> userIds, string title, string message, NotificationType type)
    {
        var notification = new
        {
            Title = title,
            Message = message,
            Type = type.ToString(),
            Timestamp = DateTime.UtcNow
        };

        var userIdStrings = userIds.Select(id => id.ToString());
        await _hubContext.Clients.Users(userIdStrings)
            .SendAsync("ReceiveNotification", notification);
    }
} 