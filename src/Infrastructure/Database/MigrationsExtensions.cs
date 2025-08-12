using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Monolith.Database;

public static class MigrationsExtensions
{
    public static IServiceCollection AddMigrationsServices(this IServiceCollection services)
    {
        services.AddSingleton<ICurrentUser, MigratorCurrentUser>();

        return services;
    }

    public static async Task MigrateDatabaseAsync<TContext>(this TContext context, ILogger logger)
        where TContext : DbContext
    {
        var dbName = context.Database.GetDbConnection().Database;

        logger.LogInformation("database {name} initializing ...", dbName);

        try
        {
            if (context.Database.GetMigrations().Any())
            {
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();

                    logger.LogInformation("database {name} initialized", dbName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}
