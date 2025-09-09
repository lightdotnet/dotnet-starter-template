global using Light.Contracts;
using Light.AspNetCore.Middlewares;
using Light.Mediator;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.HealthChecks;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.WebAdmin.Components.Account;
using Monolith.WebAdmin.Core.Auth;
using Monolith.WebAdmin.Services;
using Monolith.WebAdmin.Services.Storage;
using System.Reflection;

namespace Monolith.WebAdmin;

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

        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddScoped<IAuthService, AuthService>();

        // store token in session
        //services.AddSession();
        //services.AddDistributedMemoryCache();
        //services.AddScoped<TokenStorage, TokenSessionStorage>();

        services.AddScoped<TokenStorage, TokenCookieStorage>();
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
        services.AddScoped<ITokenProvider>(sp => (JwtAuthStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());

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

        services.AddPermissions();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        //app.UseSession();

        app.UseAuthentication(); // must be before UseAuthorization
        app.UseAuthorization();

        app.MapHealthChecksEndpoint();

        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}