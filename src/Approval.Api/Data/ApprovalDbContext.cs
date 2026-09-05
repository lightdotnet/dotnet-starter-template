using StarterKit.Approval.Api.Entities;
using StarterKit.Persistence.Context;
using StarterKit.Persistence.Extensions;
using StarterKit.Shared;

namespace StarterKit.Approval.Api.Data;

public class ApprovalDbContext(
    ICurrentUser currentUser,
    IDateTime clock,
    DbContextOptions<ApprovalDbContext> options) :
    BaseDbContext(options)
{
    public const string Schema = "approval";

    public virtual DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    public virtual DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();

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

        builder.Entity<ApprovalRequest>(entity =>
        {
            entity.ToTable(name: "ApprovalRequests");

            entity.HasIndex(x => new { x.RequestType, x.RequestId });

            entity.HasIndex(x => x.RequesterUserId);

            entity.ConfigureAuditableEntity();

            entity.Property(x => x.RequestType).HasMaxLength(100);

            entity.Property(x => x.RequestId).HasMaxLength(450);

            entity.Property(x => x.RequesterUserId).HasMaxLength(450);

            entity.Property(x => x.RequesterEmployeeId).HasMaxLength(450);

            entity.Property(x => x.Title).HasMaxLength(250);
        });

        builder.Entity<ApprovalStep>(entity =>
        {
            entity.ToTable(name: "ApprovalSteps");

            entity.HasIndex(x => x.ApprovalRequestId);

            entity.HasIndex(x => x.ApproverUserId);

            entity.ConfigureAuditableEntity();

            entity.Property(x => x.ApprovalRequestId).HasMaxLength(450);

            entity.Property(x => x.ApproverUserId).HasMaxLength(450);

            entity.Property(x => x.ApproverEmployeeId).HasMaxLength(450);

            entity.Property(x => x.Comment).HasMaxLength(1000);

            entity.HasOne(x => x.ApprovalRequest)
                .WithMany(x => x.Steps)
                .HasForeignKey(x => x.ApprovalRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
