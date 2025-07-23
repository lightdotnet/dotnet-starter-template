using Light.AspNetCore.Middlewares;
using Light.Mediator;
using Monolith.BlazorApp.Services;
using Monolith.Identity;
using Monolith.Identity.Notifications.SignalR;
using System.Reflection;

namespace Monolith.BlazorApp;

public static class ConfigureExtensions
{
    private static readonly Assembly[] assemblies =
        [
            typeof(Program).Assembly,
            typeof(IdentityModule).Assembly,
            typeof(SignalRModule).Assembly,
        ];

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Light Framework
        services.AddMediatorFromAssemblies(assemblies);
        services.AddBehaviors(typeof(ValidationBehaviour<,>));
        services.AddOptions<RequestLoggingOptions>().BindConfiguration("RequestLogging");

        services.AddInfrastructureServices();
        services.AddHealthChecks();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissions();

        return services;
    }
}