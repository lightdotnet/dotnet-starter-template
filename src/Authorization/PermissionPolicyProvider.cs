using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Monolith;

internal class PermissionPolicyProvider(
    PermissionManager permissionManager,
    IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; } = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (await permissionManager.IsValidAsync(policyName))
        {
            var policy = new AuthorizationPolicyBuilder();
            policy.AddRequirements(new PermissionRequirement(policyName));
            return policy.Build();
        }

        //return await FallbackPolicyProvider.GetPolicyAsync(policyName);
        return null;
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);
}