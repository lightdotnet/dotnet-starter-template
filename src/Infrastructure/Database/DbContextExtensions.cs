using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith.Database;

public static class DbContextExtensions
{
    public enum DbProvider
    {
        InMemory = 0,
        PostgreSQL = 1,
        MSSQL = 2,
        Sqlite = 3,
    }

    public static DbProvider GetDbProvider(this IConfiguration configuration) =>
        configuration.GetValue<DbProvider>("DbProvider");

    internal static DbContextOptionsBuilder ConfigureDatabase(this DbContextOptionsBuilder builder, DbProvider dbProvider, string connectionString)
    {
        builder
            .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));

        return dbProvider switch
        {
            DbProvider.PostgreSQL =>
                builder.UseNpgsql(connectionString).EnableSensitiveDataLogging(),
            DbProvider.MSSQL =>
                builder.UseSqlServer(connectionString),
            DbProvider.Sqlite =>
                builder.UseSqlite(connectionString),
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };
    }

    public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionName)
        where TContext : DbContext
    {
        var dbProvider = configuration.GetDbProvider();

        if (dbProvider == DbProvider.InMemory)
        {
            services.AddDbContext<TContext>(options => options.UseInMemoryDatabase($"InMemoryDb"));
        }
        else
        {
            var connectionString = configuration.GetConnectionString(connectionName)
                ?? throw new InvalidOperationException($"{connectionName} connection string is not configure.");

            services.AddDbContext<TContext>(options =>
            {
                options.ConfigureDatabase(dbProvider, connectionString);
            });
        }

        StaticLogger.Write("DbContext {name} configured with {dbProvider} provider.", typeof(TContext).Name, dbProvider);

        return services;
    }
}
