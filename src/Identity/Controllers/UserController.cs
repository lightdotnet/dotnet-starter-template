using Light.ActiveDirectory.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Monolith.Identity.Controllers;

[MustHavePermission(Permissions.Users.View)]
public class UserController(
    IUserService userService,
    IActiveDirectoryService activeDirectoryService) : ApiControllerBase
{
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
    [MustHavePermission(Permissions.Users.Create)]
    public async Task<IActionResult> PostAsync([FromBody] CreateUserRequest request)
    {
        var res = await userService.CreateAsync(request);
        return Ok(res);
    }

    [HttpPut("{id}")]
    [MustHavePermission(Permissions.Users.Update)]
    public async Task<IActionResult> PutAsync(string id, UserDto request)
    {
        if (id != request.Id)
        {
            return Ok(Result.Error("Validate User ID not match"));
        }

        return Ok(await userService.UpdateAsync(request));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(Permissions.Users.Delete)]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        return Ok(await userService.DeleteAsync(id));
    }

    [HttpPut("{id}/password/force")]
    [MustHavePermission(Permissions.Users.Update)]
    public async Task<IActionResult> ForcePasswordAsync(string id, [FromBody] string password)
    {
        return Ok(await userService.ForcePasswordAsync(id, password));
    }

    [HttpGet("get_domain_user/{userName}")]
    public async Task<IActionResult> GetDomainUserAsync([FromRoute] string userName)
    {
        return Ok(await activeDirectoryService.GetByUserNameAsync(userName));
    }

    [HttpPatch("sync_domain_users")]
    public async Task<IActionResult> SyncDomainUsersAsync()
    {
        var users = await userService.GetAllAsync();

        var domainUsers = users.Where(x => x.AuthProvider == AuthProvider.AD.ToString());

        foreach (var user in domainUsers)
        {
            var domainUser = await activeDirectoryService.GetByUserNameAsync(user.UserName);
            if (domainUser is null)
            {
                user.Status = IdentityStatus.locked;

                await userService.UpdateAsync(user);
            }
        }

        return Ok();
    }
}
