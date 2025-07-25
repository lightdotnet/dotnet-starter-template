global using Light.Identity.Models;

using HealthChecks.UI.Client;
using Light.AspNetCore.Builder;
using Light.AspNetCore.Middlewares;
using Light.Extensions.DependencyInjection;
using Light.Mediator;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Monolith.BlazorServer.Services;
using Monolith.Identity;
using Monolith.Identity.Notifications.SignalR;
using Monolith.Modularity;
using System.Reflection;

namespace Monolith.BlazorServer;

public static class ConfigureExtensions
{
    private static readonly Assembly[] assemblies =
        [
            typeof(Program).Assembly,
            typeof(SignalRModule).Assembly,
        ];

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Light Framework
        services.AddMediatorFromAssemblies(assemblies);
        services.AddBehaviors(typeof(ValidationBehaviour<,>));
        services.AddOptions<RequestLoggingOptions>().BindConfiguration("RequestLogging");
        services.AddModules<AppModule>(configuration, assemblies);

        services.AddInfrastructureServices();
        services.AddHealthChecks();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissions();

        services
            .AddIdentityModule(configuration)
            .AddSignInManager();

        // custom Claims for Identity user
        services.AddScoped<IUserClaimsPrincipalFactory<User>, AppUserClaimsPrincipalFactory>();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        app.UseModules<AppModule>(assemblies);

        app.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapModuleEndpoints<AppHub>(assemblies);

        return app;
    }
}