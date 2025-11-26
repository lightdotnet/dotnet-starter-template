using Light.AspNetCore.Authorization;
using Light.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Monolith.Authorization.Internal;

internal class AuthorizationHandler : PermissionAuthorizationHandler
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;

        var isSuperUser = AppSecret.IsSuper(user.GetUserName());

        var hasPermission = user.HasPermission(requirement.Permission);

        if (isSuperUser || hasPermission)
        {
            context.Succeed(requirement);
        }

        await Task.CompletedTask;
    }
}