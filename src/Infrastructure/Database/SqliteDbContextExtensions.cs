using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Monolith.Database;

public static class SqliteDbContextExtensions
{
    public static void FixDateTimeOffsetSqlite(this DatabaseFacade database, ModelBuilder modelBuilder)
    {
        if (database.IsSqlite())
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var dateTimeOffsetProps = entityType
                    .GetProperties()
                    .Where(p =>
                        p.ClrType == typeof(DateTimeOffset)
                        || p.ClrType == typeof(DateTimeOffset?));

                foreach (var property in dateTimeOffsetProps)
                {
                    if (property.ClrType == typeof(DateTimeOffset))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTimeOffset, long>(
                                v => v.ToUnixTimeSeconds(),
                                v => DateTimeOffset.FromUnixTimeSeconds(v)));
                    }
                    else if (property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(
                            new ValueConverter<DateTimeOffset?, long>(
                                v => v.HasValue ? v.Value.ToUnixTimeSeconds() : 0,
                                v => v > 0 ? DateTimeOffset.FromUnixTimeSeconds(v) : default));
                    }
                }
            }
        }
    }
}
