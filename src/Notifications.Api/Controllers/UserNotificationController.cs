using Microsoft.AspNetCore.Mvc;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Notifications.Contracts.SystemNotifications;
using StarterKit.Shared;

namespace StarterKit.Notifications.Api.Controllers;

[ApiExplorerSettings(GroupName = "push")]
public class UserNotificationController(
    INotificationService notificationService,
    ICurrentUser currentUser) : VersionedApiController
{
    private readonly string _currentUserId = currentUser.UserId
        ?? throw new ArgumentNullException(nameof(currentUser.UserId));

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] NotificationLookup request)
    {
        request.ToUserId = _currentUserId;
        
        var res = await notificationService.GetAsync(request);

        return Ok(res);
    }

    [HttpGet("{entryId}")]
    public async Task<IActionResult> Get(string entryId)
    {
        var toUserId = _currentUserId;

        await notificationService.MarkAsReadAsync(toUserId, entryId);

        var res = await notificationService.GetByIdAsync(toUserId, entryId);

        return Ok(res);
    }

    [HttpGet("count_unread")]
    public async Task<IActionResult> CountUnread()
    {
        var toUserId = _currentUserId;

        var res = await notificationService.CountUnreadAsync(toUserId);

        return Ok(res);
    }
}