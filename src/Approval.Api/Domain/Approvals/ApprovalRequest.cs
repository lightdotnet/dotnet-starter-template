using StarterKit.Shared.Entities;

namespace StarterKit.Approval.Api.Domain.Approvals;

public class ApprovalRequest : AuditableEntity
{
    public string RequestType { get; set; } = null!;

    public string RequestId { get; set; } = null!;

    public string RequesterUserId { get; set; } = null!;

    /// <summary>
    /// Opaque bookkeeping reference to the requester's Organization employee record.
    /// Null when the requester's account is not linked to an employee.
    /// </summary>
    public string? RequesterEmployeeId { get; set; }

    /// <summary>
    /// Display label for the requester, captured at creation time by the calling module.
    /// Approval has no view onto identity/organization data, so it cannot resolve this itself.
    /// </summary>
    public string? RequesterName { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? DeepLinkUrl { get; set; }

    public string? DocumentTypeId { get; set; }

    public ApprovalDocumentType? DocumentType { get; set; }

    public int CurrentLevel { get; set; } = 1;

    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

    public DateTimeOffset? FinalizedAt { get; set; }

    public IList<ApprovalStep> Steps { get; set; } = [];
}
