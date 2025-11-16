using Microsoft.EntityFrameworkCore;

namespace Monolith.Database;

public abstract class BaseDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        Database.FixDateTimeOffsetSqlite(modelBuilder);
    }
}
