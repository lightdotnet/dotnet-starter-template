using Light.Identity;
using Light.Identity.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Monolith;
using Monolith.Database;
using Monolith.Identity.Data;
using Monolith.Infrastructure.Data;
using Monolith.Infrastructure.Data.SeedWork;
using System.Reflection;

namespace MSSQL;

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
                .UseSqlServer(connectionString, o =>
                {
                    o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddIdentity<AppIdentityDbContext>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie();
        
        services.AddScoped<IdentityContextInitialiser>();

        return services;
    }

    private static IServiceCollection AddCatalog(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(DbConnectionNames.DEFAULT);

        services.AddDbContext<CatalogContext>(options =>
            options
                .UseSqlServer(connectionString, o =>
                {
                    o.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                })
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

        services.AddScoped<CatalogContextInitialiser>();

        return services;
    }
}
