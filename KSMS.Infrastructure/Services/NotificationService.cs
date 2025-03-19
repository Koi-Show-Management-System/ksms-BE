using System.Linq.Expressions;
using KSMS.Application.Repositories;
using KSMS.Application.Services;
using KSMS.Domain.Dtos.Responses.Notification;
using KSMS.Domain.Entities;
using KSMS.Domain.Enums;
using KSMS.Domain.Exceptions;
using KSMS.Domain.Pagination;
using KSMS.Infrastructure.Database;
using KSMS.Infrastructure.Hubs;
using KSMS.Infrastructure.Utils;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace KSMS.Infrastructure.Services;

public class NotificationService : BaseService<NotificationService>, INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    
    public NotificationService(IUnitOfWork<KoiShowManagementSystemContext> unitOfWork, ILogger<NotificationService> logger, IHttpContextAccessor httpContextAccessor, IHubContext<NotificationHub> hubContext) : base(unitOfWork, logger, httpContextAccessor)
    {
        _hubContext = hubContext;
    }
    

    public async Task SendNotification(Guid accountId, string title, string message, NotificationType type)
    {
        var notification = new Notification()
        {
            AccountId = accountId,
            Title = title,
            Content = message,
            Type = type.ToString(),
            SentDate = VietNamTimeUtil.GetVietnamTime(),
            IsRead = false
        };
        
        await _unitOfWork.GetRepository<Notification>().InsertAsync(notification);
        await _unitOfWork.CommitAsync();
        var notificationResponse = new
        {
            Id = notification.Id,
            Title = title,
            Message = message,
            Type = type.ToString(),
            Timestamp = notification.SentDate
        };
        await _hubContext.Clients.Group(accountId.ToString())
            .SendAsync("ReceiveNotification", notificationResponse);
    }

    public async Task SendNotificationToMany(List<Guid> accountIds, string title, string message, NotificationType type)
    {
        var notifications = accountIds.Select(accountId => new Notification
        {
            AccountId = accountId,
            Title = title,
            Content = message,
            Type = type.ToString(),
            SentDate = VietNamTimeUtil.GetVietnamTime(),
            IsRead = false
        }).ToList();
        await _unitOfWork.GetRepository<Notification>().InsertRangeAsync(notifications);
        await _unitOfWork.CommitAsync();
        var notificationResponse = new
        {
            Title = title,
            Message = message,
            Type = type.ToString(),
            Timestamp = VietNamTimeUtil.GetVietnamTime()
        };
        var userIdStrings = accountIds.Select(id => id.ToString());
        foreach (var accountId in accountIds)
        {
            await _hubContext.Clients.Group(accountId.ToString())
                .SendAsync("ReceiveNotification", notificationResponse);
        }
        
    }

    public async Task MarkNotificationAsRead(Guid notificationId)
    {
        var notification = await _unitOfWork.GetRepository<Notification>()
            .SingleOrDefaultAsync(predicate: x => x.Id == notificationId);
        if (notification == null)
        {
            throw new NotFoundException($"Notification with ID {notificationId} not found.");
        }
        notification.IsRead = true;
        _unitOfWork.GetRepository<Notification>().UpdateAsync(notification);
        await _unitOfWork.CommitAsync();
    }

    public async Task MarkAllNotificationAsRead(Guid accountId)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.Id == accountId);
        if (account is null)
        {
            throw new NotFoundException("Account is not exist");
        }
        var notifications = await _unitOfWork.GetRepository<Notification>()
            .GetListAsync(predicate: x => x.AccountId == accountId && x.IsRead == false);
        if (notifications.Any())
        {
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
           
            }
            _unitOfWork.GetRepository<Notification>().UpdateRange(notifications);
            await _unitOfWork.CommitAsync();
        }
        
    }

    public async Task<Paginate<GetPageNotificationResponse>> GetPageNotification(Guid accountId, bool? isRead,
        NotificationType? notificationType, int page, int size)
    {
        var account = await _unitOfWork.GetRepository<Account>().SingleOrDefaultAsync(
            predicate: x => x.Id == accountId);
        if (account is null)
        {
            throw new NotFoundException("Account is not exist");
        }
        Expression<Func<Notification, bool>> predicate = x => x.AccountId == accountId;
        if (isRead.HasValue)
        {
            predicate = predicate.And(x => x.IsRead == isRead.Value);
        }
        if (notificationType.HasValue)
        {
            predicate = predicate.And(x => x.Type == notificationType.ToString());
        }
        var notifications = await _unitOfWork.GetRepository<Notification>()
            .GetPagingListAsync(predicate: predicate,
                orderBy: q => q.OrderByDescending(n => n.SentDate),
                page: page,
                size: size);
        return notifications.Adapt<Paginate<GetPageNotificationResponse>>();
    }

    
} 