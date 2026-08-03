using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Notifications.Api.SignalR;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Notifications.Contracts.SystemNotifications;

namespace StarterKit.Notifications.Api.Controllers;

[ApiExplorerSettings(GroupName = "push")]
public class NotificationController(
    IUserService userService,
    IHubService hub,
    INotificationService notificationService) : VersionedApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] NotificationLookup request)
    {
        var res = await notificationService.GetAsync(request);
        return Ok(res);
    }

    [HttpPost]
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

    [AllowAnonymous]
    [BasicAuth]
    [HttpPost("send_by_user_claim")]
    public async Task<IActionResult> SendByUserClaim(
        [FromQuery] string claimType,
        [FromQuery] string claimValue,
        [FromBody] SystemMessage request)
    {
        var getUsers = await userService.GetUsersHasClaimAsync(claimType, claimValue);

        foreach (var user in getUsers)
        {
            await notificationService.SaveAsync("system", "system", user.Id, request);

            // send notify after save record for load notification entries from API when receive
            await hub.SendAsync(request, user.Id);
        }

        return Ok();
    }

    [HttpPost("force_logout")]
    public async Task<IActionResult> ForceLogout([FromBody] ForceLogoutMessage request)
    {
        await hub.SendAsync(request, request.UserId);

        return Ok();
    }
}