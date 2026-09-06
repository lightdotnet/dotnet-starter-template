using StarterKit.Shared.Entities;

namespace StarterKit.Approval.Api.Domain.Approvals;

public class ApprovalStep : AuditableEntity
{
    public string ApprovalRequestId { get; set; } = null!;

    public int Level { get; set; }

    public string ApproverUserId { get; set; } = null!;

    public string ApproverEmployeeId { get; set; } = null!;

    /// <summary>
    /// Display label for this approver, captured at creation time by the calling module.
    /// Approval has no view onto identity/organization data, so it cannot resolve this itself.
    /// </summary>
    public string? ApproverName { get; set; }

    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;

    public string? Comment { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    public ApprovalRequest ApprovalRequest { get; set; } = null!;
}
