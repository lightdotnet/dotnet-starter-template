using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public class PermissionAuthorizationHandler(ICurrentUser currentUser) : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var canAccess = currentUser.HasPermission(requirement.Permission);
        if (canAccess)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}