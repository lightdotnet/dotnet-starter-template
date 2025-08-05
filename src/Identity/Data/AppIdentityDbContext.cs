using Light.Identity.EntityFrameworkCore;
using Monolith.Database;
using Monolith.Identity.Models;

namespace Monolith.Identity.Data;

public class AppIdentityDbContext(
    ICurrentUser currentUser,
    IDateTime clock,
    DbContextOptions<AppIdentityDbContext> options) :
    IdentityContext(options)
{
    public virtual DbSet<Notification> Notifications => Set<Notification>();

    public override int SaveChanges()
    {
        this.AuditEntries(currentUser.UserId, clock.Now, false);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.AuditEntries(currentUser.UserId, clock.Now, false);
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Notification>().ToTable(name: "Notifications", Schemas.System);

        Database.FixDateTimeOffsetSqlite(builder);
    }
}