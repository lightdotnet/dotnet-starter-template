using Light.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith.Authorization.Internal;

public static class DependencyInjection
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services) =>
        services
            .AddPermissionPolicyProvider<PolicyProvider>()
            .AddPermissionAuthorizationHandler<AuthorizationHandler>();
}