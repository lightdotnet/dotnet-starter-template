using Light.AspNetCore.Middlewares;
using Light.Mediator;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.BlazorServer.Components.Account;
using Monolith.BlazorServer.Core.Auth;
using Monolith.BlazorServer.Services;
using Monolith.HealthChecks;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;
using System.Reflection;

namespace Monolith.BlazorServer;

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

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, ServerCurrentUser>();

        /*
        services.AddDistributedMemoryCache();
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromDays(6);
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.MaxAge = TimeSpan.FromDays(6);
        });
        */

        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
        //services.AddScoped<ITokenProvider, TokenProvider>();
        //services.AddScoped<ITokenProvider, TokenSessionProvider>();
        services.AddScoped<ITokenProvider, TokenCookieProvider>();

        services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
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

        services.AddPermissions();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        //app.UseSession();

        //app.UseAuthentication(); // must be before UseAuthorization
        //app.UseAuthorization();

        app.MapHealthChecksEndpoint();

        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}