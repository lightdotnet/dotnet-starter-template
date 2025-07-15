using Light.Identity.EntityFrameworkCore;
using Monolith.Database;

namespace Monolith.Identity.Data;

public class AppIdentityDbContext(
    ICurrentUser currentUser,
    IDateTime clock,
    DbContextOptions<AppIdentityDbContext> options) :
    IdentityContext(options)
{
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
    }
}