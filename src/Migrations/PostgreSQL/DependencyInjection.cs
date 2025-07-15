using Light.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith;
using Monolith.Database;
using Monolith.Identity.Data;
using Monolith.Infrastructure.Data;
using Monolith.Infrastructure.Data.SeedWork;
using System.Reflection;

namespace PostgreSQL;

public static class DependencyInjection
{
    public static IServiceCollection AddMigrator(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructureServices();

        services.AddMigratorServices();

        services.AddIdentity(configuration);

        services.AddCatalog(configuration);

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DbConnectionNames.IDENTITY);

        services.AddDbContext<AppIdentityDbContext>(options =>
            options
                .UseNpgsql(connectionString, o =>
                {
                    o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddIdentity<AppIdentityDbContext>();

        services.AddScoped<IdentityContextInitialiser>();

        return services;
    }

    private static IServiceCollection AddCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DbConnectionNames.DEFAULT);

        services.AddDbContext<CatalogContext>(options =>
            options
                .UseNpgsql(connectionString, o =>
                {
                    o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<CatalogContextInitialiser>();

        return services;
    }
}
