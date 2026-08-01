using Light.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Identity.Api.Application.Users.Commands;
using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Shared;

namespace StarterKit.Identity.Api.Controllers;

[ApiExplorerSettings(GroupName = "identity")]
[MustHavePermission(IdentityPermissions.Users.View)]
public class UserController(
    IUserService userService,
    IActiveDirectoryService activeDirectoryService) : VersionedApiController
{
    [HttpPost("search")]
    public async Task<IActionResult> SearchAsync(
        [FromQuery] SearchUserQuery search,
        [FromQuery] PageQuery page)
    {
        return Ok(await userService.SearchAsync(search, page.PageNumber, page.PageSize));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        return Ok(await userService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(string id)
    {
        return Ok(await userService.GetByIdAsync(id));
    }

    [HttpGet("by_username/{username}")]
    public async Task<IActionResult> GetByUsernameAsync(string username)
    {
        return Ok(await userService.GetByUserNameAsync(username));
    }

    [HttpPost]
    [MustHavePermission(IdentityPermissions.Users.Create)]
    public async Task<IActionResult> PostAsync([FromBody] CreateUserCommand request)
    {
        var res = await Mediator.Send(request);
        return Ok(res);
    }

    [HttpPut("{id}")]
    [MustHavePermission(IdentityPermissions.Users.Update)]
    public async Task<IActionResult> PutAsync(string id, [FromBody] UserDto request)
    {
        if (id != request.Id)
        {
            return Ok(Result.Error("Validate User ID not match"));
        }

        return Ok(await userService.UpdateAsync(request));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(IdentityPermissions.Users.Delete)]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        return Ok(await userService.DeleteAsync(id));
    }

    [HttpPut("{id}/password/force")]
    [MustHavePermission(IdentityPermissions.Users.Update)]
    public async Task<IActionResult> ForcePasswordAsync(string id, [FromBody] string password)
    {
        return Ok(await userService.ForcePasswordAsync(id, password));
    }

    [HttpGet("get_domain_user/{userName}")]
    public async Task<IActionResult> GetDomainUserAsync([FromRoute] string userName)
    {
        return Ok(await activeDirectoryService.GetByUserNameAsync(userName));
    }

    [HttpPut("sync_domain_users")]
    public async Task<IActionResult> SyncDomainUsersAsync()
    {
        var users = await userService.GetAllAsync();

        var domainUsers = users.Where(x => x.AuthProvider == AuthProvider.AD.ToString());

        foreach (var user in domainUsers)
        {
            var domainUser = await activeDirectoryService.GetByUserNameAsync(user.UserName);
            if (domainUser is null)
            {
                user.Status = ActiveStatus.State.Locked.ToString();

                await userService.UpdateAsync(user);
            }
        }

        return Ok();
    }
}
