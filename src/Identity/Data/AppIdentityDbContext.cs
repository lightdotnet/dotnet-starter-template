using Light.Identity.EntityFrameworkCore;
using Monolith.Database;

namespace Monolith.Identity.Data;

public class AppIdentityDbContext(
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    DbContextOptions<AppIdentityDbContext> options) :
    IdentityContext(options)
{
    public override int SaveChanges()
    {
        var now = timeProvider.GetUtcNow();
        this.AuditEntries(currentUser.UserId, now, false);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();
        this.AuditEntries(currentUser.UserId, now, false);
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}