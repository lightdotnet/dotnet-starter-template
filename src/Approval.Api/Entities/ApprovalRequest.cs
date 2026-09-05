using StarterKit.Shared.Entities;

namespace StarterKit.Approval.Api.Entities;

public class ApprovalRequest : AuditableEntity
{
    public string RequestType { get; set; } = null!;

    public string RequestId { get; set; } = null!;

    public string RequesterUserId { get; set; } = null!;

    public string RequesterEmployeeId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? DeepLinkUrl { get; set; }

    public int CurrentLevel { get; set; } = 1;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTimeOffset? FinalizedAt { get; set; }

    public IList<ApprovalStep> Steps { get; set; } = [];
}
