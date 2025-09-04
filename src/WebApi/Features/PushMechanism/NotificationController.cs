using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Monolith.Endpoints;
using Monolith.Identity.Notifications;
using Monolith.Notifications;

namespace Monolith.Features.PushMechanism;

[ApiExplorerSettings(GroupName = "Push")]
public class NotificationController(
    ICurrentUser currentUser,
    IUserService userService,
    IHubContext hub,
    INotificationService notificationService) : VersionedApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] NotificationLookup request)
    {
        var canViewAll = currentUser.HasPermission(Permissions.System.Notification);

        if (!canViewAll) // view all when uses can send notify
        {
            request.ToUser = currentUser.UserId;
        }

        var res = await notificationService.GetAsync(request);

        return Ok(res);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        var res = await notificationService.GetByIdAsync(id);

        return Ok(res);
    }

    [HttpGet("{userId}/unread/count")]
    public async Task<IActionResult> CountUnreadAsync(string userId)
    {
        var res = await notificationService.CountUnreadAsync(userId);
        return Ok(res);
    }

    [HttpGet("read/{id}")]
    public async Task<IActionResult> ReadAsync(string id)
    {
        await notificationService.ReadAsync(id);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SendToUserId(string fromUserId, string? fromName, string toUserId, [FromBody] SystemMessage request)
    {
        await notificationService.SaveAsync(fromUserId, fromName, toUserId, request);

        // send notify after save record for load notification entries from API when receive
        await hub.SendAsync(toUserId, request);

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
        var getUsers = await userService.GetUsersHasClaim(claimType, claimValue);

        foreach (var user in getUsers)
        {
            await notificationService.SaveAsync("System", null, user.Id, request);

            // send notify after save record for load notification entries from API when receive
            await hub.SendAsync(user.Id, request);
        }

        return Ok();
    }

    [HttpPost("force_logout")]
    public async Task<IActionResult> ForceLogout([FromBody] ForceLogoutMessage request)
    {
        await hub.SendAsync(request.UserId, request);

        return Ok();
    }
}