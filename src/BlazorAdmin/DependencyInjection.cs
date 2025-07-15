using BlazorAdmin.Components.Pages.Account;
using BlazorAdmin.Services;
using Light.FluentBlazor;
using Light.Identity;
using Light.Identity.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Monolith;
using Monolith.Database;
using Monolith.Identity.Data;

namespace BlazorAdmin;

internal static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFluentBlazorExtraComponents();

        services.AddInfrastructureServices();

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(DbConnectionNames.IDENTITY)));

        services.AddIdentity<AppIdentityDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/access-denied";
        });

        services.AddScoped<LoginService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissions();

        //custom Claims for Identity user
        services.AddScoped<IUserClaimsPrincipalFactory<User>, AppUserClaimsPrincipalFactory>();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
