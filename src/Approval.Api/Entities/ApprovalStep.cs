using StarterKit.Shared.Entities;

namespace StarterKit.Approval.Api.Entities;

public class ApprovalStep : AuditableEntity
{
    public string ApprovalRequestId { get; set; } = null!;

    public int Level { get; set; }

    public string ApproverUserId { get; set; } = null!;

    public string ApproverEmployeeId { get; set; } = null!;

    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;

    public string? Comment { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;
}
