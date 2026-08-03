using Light.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Notifications.Api.SignalR;
using StarterKit.Notifications.Contracts.Authorization;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Notifications.Contracts.SystemNotifications;

namespace StarterKit.Notifications.Api.Controllers;

[ApiExplorerSettings(GroupName = "push")]
public class NotificationController(
    IHubService hub,
    INotificationService notificationService) : VersionedApiController
{
    [HttpGet]
    [MustHavePermission(NotificationPermissions.Read)]
    public async Task<IActionResult> GetAsync([FromQuery] NotificationLookup request)
    {
        var res = await notificationService.GetAsync(request);
        return Ok(res);
    }

    [HttpPost]
    [MustHavePermission(NotificationPermissions.Send)]
    public async Task<IActionResult> SendToUserId(
        string fromUserId,
        string? fromName,
        string toUserId,
        [FromBody] SystemMessage request)
    {
        await notificationService.SaveAsync(fromUserId, fromName, toUserId, request);

        // send notify after save record for load notification entries from API when receive
        // *** note: must send message include to WebClient for client consume
        await hub.SendAsync(request, toUserId);

        return Ok();
    }

    [HttpPost("force_logout")]
    [MustHavePermission(NotificationPermissions.Send)]
    public async Task<IActionResult> ForceLogout([FromBody] ForceLogoutMessage request)
    {
        await hub.SendAsync(request, request.UserId);

        return Ok();
    }
}