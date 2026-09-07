using StarterKit.LeaveManagement.Api.Domain.LeaveRequests;
using StarterKit.Persistence.Context;
using StarterKit.Persistence.Extensions;
using StarterKit.Shared;

namespace StarterKit.LeaveManagement.Api.Data;

public class LeaveManagementDbContext(
    ICurrentUser currentUser,
    IDateTime clock,
    DbContextOptions<LeaveManagementDbContext> options) :
    BaseDbContext(options)
{
    public const string Schema = "leave";

    public virtual DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    public override int SaveChanges()
    {
        this.AuditEntries(currentUser.UserId, clock.AuditTime, false);
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.AuditEntries(currentUser.UserId, clock.AuditTime, false);
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void ConfigureModel(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        builder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable(name: "LeaveRequests");

            entity.HasIndex(x => new { x.EmployeeId, x.Status });

            entity.ConfigureAuditableEntity();

            entity.Property(x => x.UserId).HasMaxLength(450);

            entity.Property(x => x.EmployeeId).HasMaxLength(450);

            entity.Property(x => x.Reason).HasMaxLength(1000);

            entity.Property(x => x.ApprovalRequestId).HasMaxLength(450);
        });
    }
}
