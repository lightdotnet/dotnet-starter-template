using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith.Database;

public static class DbContextExtensions
{
    public enum DbProviders
    {
        InMemory = 0,
        PostgreSQL = 1,
        MSSQL = 2,
    }

    internal static DbContextOptionsBuilder ConfigureDatabase(this DbContextOptionsBuilder builder, DbProviders dbProvider, string connectionString)
    {
        builder
            .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning));

        return dbProvider switch
        {
            DbProviders.PostgreSQL =>
                builder.UseNpgsql(connectionString).EnableSensitiveDataLogging(),
            DbProviders.MSSQL =>
                builder.UseSqlServer(connectionString),
            _ => throw new InvalidOperationException($"DB Provider {dbProvider} is not supported."),
        };
    }

    public static IServiceCollection AddDbContext<TContext>(this IServiceCollection services, IConfiguration configuration, string connectionName)
        where TContext : DbContext
    {
        var dbProvider = configuration.GetValue<DbProviders>("DbProvider");

        if (dbProvider == DbProviders.InMemory)
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

        return services;
    }
}
