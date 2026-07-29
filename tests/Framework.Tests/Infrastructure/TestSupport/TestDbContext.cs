using Microsoft.EntityFrameworkCore;

namespace Framework.Tests.Infrastructure.TestSupport;

public sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestAggregate>().OwnsOne(a => a.Note);
    }

    public static TestDbContext CreateInMemory(string? databaseName = null) =>
        new(new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options);
}
