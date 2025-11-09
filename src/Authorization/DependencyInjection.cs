using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith;

public static class DependencyInjection
{
    public static IServiceCollection AddDefaultPermissionManager(this IServiceCollection services) =>
        services.AddSingleton<PermissionManager>();

    public static IServiceCollection AddPermissionManager<T>(this IServiceCollection services)
        where T : PermissionManager
    {
        return services.AddSingleton<PermissionManager, T>();
    }

    public static IServiceCollection AddPermissionPolicy(this IServiceCollection services) =>
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services) =>
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}