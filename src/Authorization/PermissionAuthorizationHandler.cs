using Microsoft.AspNetCore.Authorization;
using Monolith.Extensions;

namespace Monolith;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;

        bool isSuperUser = AppSecret.IsSuper(user.GetUserName());

        bool hasPermission = user.HasPermission(requirement.Permission);

        if (isSuperUser || hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}