using BlazorAdmin.Services;
using Light.FluentBlazor;
using Light.Identity;
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
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissions();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
