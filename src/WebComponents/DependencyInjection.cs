using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Identity;
using Monolith.Services;

namespace Monolith;

public static class ConfigureExtensions
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices();
        services.AddHealthChecks();

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, ServerCurrentUser>();

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

        return services;
    }
}