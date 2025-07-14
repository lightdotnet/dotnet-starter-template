using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public class PermissionAuthorizationHandler(ICurrentUser currentUser) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (currentUser.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        await Task.CompletedTask;
    }
}