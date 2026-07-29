using Light.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Authorization.Internal;

namespace StarterKit.Authorization;

public static class DependencyInjection
{
    public static IServiceCollection AddPermissionPolicies(this IServiceCollection services) =>
        services.AddPermissionPolicyProvider<PolicyProvider>();

    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services) =>
        services.AddPermissionAuthorizationHandler<AuthorizationHandler>();
}