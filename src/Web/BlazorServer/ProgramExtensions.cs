using Light.Extensions.DependencyInjection;
using Light.MudBlazor;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.Infrastructure;
using Monolith.Infrastructure.Services;
using Monolith.Infrastructure.Storage;
using Monolith.Services;
using MudBlazor.Services;

namespace Monolith;

public static class ProgramExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMudServices();
        services.AddMudBlazorExtraComponents();

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddFileGenerator();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();

        //services.AddDefaultPermissionManager();
        services.AddPermissionManager<AppPermissionManager>();
        services.AddPermissionPolicy();
        services.AddPermissionAuthorization();

        services.AddTransient<ITokenProvider, TokenProvider>();

        // store token in session
        //services.AddSession();
        //services.AddDistributedMemoryCache();
        //services.AddScoped<TokenStorage, TokenSessionStorage>();
        //services.AddScoped<TokenStorage, TokenLocalStorage>();
        services.AddScoped<TokenStorage, TokenCookieStorage>();

        services.AddScoped<IAuthService, AuthService>();

        services
            .AddAuthentication("jwt")
            .AddCookie("jwt", options =>
            {
                //options.Cookie.Name = "mini-session";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(6);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/access-denied";
            });

        return services;
    }
}