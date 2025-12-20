using Light.Extensions.DependencyInjection;
using Light.MudBlazor;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Authorization;
using Monolith.Blazor.Components.Account;
using Monolith.Blazor.Services;
using Monolith.Blazor.Services.Token;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFileGenerator();
        services.AddMudBlazorExtraComponents();

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddCascadingAuthenticationState();

        services.AddScoped<LayoutService>();
        services.AddScoped<SignalRClient>();

        services.AddHttpContextAccessor();

        // WebServer
        services.AddSingleton<TokenMemoryStorage>();
        services.AddScoped<TokenStorage, TokenCookieStorage>();

        services
            .AddAuthentication(Constants.JwtAuthScheme)
            .AddCookie(Constants.JwtAuthScheme, options =>
            {
                options.Cookie.Name = "__user";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(6);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/account/login";
                //options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/access-denied";
            });

        services.AddScoped<IClientCurrentUser, CurrentUser1>();

        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProviderServer>();
        services.AddScoped(sp => (ITokenProvider)sp.GetRequiredService<AuthenticationStateProvider>());

        services.AddPermissionPolicies();
        services.AddPermissionAuthorization();

        return services;
    }

    public static WebApplication AddWebPipelines(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAdditionalIdentityEndpoints();

        return app;
    }
}
