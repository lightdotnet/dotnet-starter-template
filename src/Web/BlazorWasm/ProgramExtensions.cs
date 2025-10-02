global using Light.Blazor;
global using Monolith;

using Blazored.LocalStorage;
using Light.Extensions.DependencyInjection;
using Light.MudBlazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Core;
using Monolith.Core.Auth;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.Infrastructure;

namespace Monolith;

public static class ProgramExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFileGenerator();

        services.AddBlazoredLocalStorageAsSingleton();
        services.AddSingleton<IStorageService, StorageService>();

        services.AddSingleton<IWebSettings, WebSettings>();

        services.AddScoped<SignalRClient>();
        services.AddScoped<LayoutService>();

        services.AddScoped<IAppTokenService, AppTokenService>();

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddMudBlazorExtraComponents();

        services.AddAuth();

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthorizationCore(RegisterPermissions);
        services.AddCascadingAuthenticationState();
        // register the custom state provider
        services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
        services.AddScoped(sp => (IIdentityManager)sp.GetRequiredService<AuthenticationStateProvider>());
        services.AddScoped(sp => (ITokenProvider)sp.GetRequiredService<AuthenticationStateProvider>());
        services.AddScoped<ICurrentUser, ClientCurrentUser>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    private static void RegisterPermissions(AuthorizationOptions options)
    {
        var permissions = ClientClaimsExtensions.GetAll().Claims.Select(s => s.Value).ToList();

        foreach (var permission in permissions)
        {
            options.AddPolicy(permission, policy =>
                policy.AddRequirements(new PermissionRequirement(permission)));
        }
    }
}