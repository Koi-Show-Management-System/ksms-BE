using KSMS.Application.Services;
using KSMS.Domain.Dtos;
using KSMS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace KSMS.API.Controllers;
[Route("api/v1/notification")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    [HttpGet("get-page/{accountId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> GetPage(Guid accountId, [FromQuery]bool? isRead,
        [FromQuery] NotificationType? notificationType,
        [FromQuery] int page = 1, [FromQuery]int size  = 10)
    {
        var notifications = await _notificationService.GetPageNotification(accountId, isRead, notificationType, page, size);
        return Ok(ApiResponse<object>.Success(notifications, "Get list successfully"));
    }
    [HttpPatch("mark-as-read{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id)
    {
        await _notificationService.MarkNotificationAsRead(id);
        return Ok(ApiResponse<object>.Success(null, "Mark as read successfully"));
    }
    [HttpPatch("mark-as-read-all/{accountId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsReadAll(Guid accountId)
    {
        await _notificationService.MarkAllNotificationAsRead(accountId);
        return Ok(ApiResponse<object>.Success(null, "Mark all as read successfully"));
    }
}