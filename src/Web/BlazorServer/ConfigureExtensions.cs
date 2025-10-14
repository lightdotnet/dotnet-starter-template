global using Light.Contracts;

using Light.AspNetCore.Middlewares;
using Light.Mediator;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Monolith.Components.Account;
using Monolith.HealthChecks;
using Monolith.Identity;
using Monolith.Services;
using System.Reflection;

namespace Monolith;

public static class ConfigureExtensions
{
    private static readonly Assembly[] assemblies =
        [
            typeof(Program).Assembly,
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

        services.AddScoped<IAuthService, AuthService>();
        
        services
            .AddAuthentication("__identity")
            .AddCookie("__identity", options =>
            {
                options.Cookie.Name = "mini-session";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(6);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/access-denied";
            });

        services
            .AddIdentityServices(configuration)
            .AddSignInManager();

        services.AddPermissionStores<AppPermissionManager>();
        services.AddPermissionPolicy();
        services.AddPermissionAuthorization();

        //services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        //services.AddCascadingAuthenticationState();
        //services.AddAuthenticationStateDeserialization();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        //app.UseAuthentication(); // must be before UseAuthorization
        //app.UseAuthorization();

        app.MapHealthChecksEndpoint();

        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}