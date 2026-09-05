namespace StarterKit.Approval.Contracts.Approvals;

/// <summary>
/// Input for <see cref="Services.IApprovalService.CreateAsync"/>. The calling module is
/// responsible for resolving <see cref="ApproverChain"/> (Approval has no knowledge of
/// org structure, employee levels, etc.) so this same shape works for any future request type.
/// </summary>
public record CreateApprovalRequest(
    string RequestType,
    string RequestId,
    string RequesterUserId,
    string RequesterEmployeeId,
    string Title,
    string? DeepLinkUrl,
    IReadOnlyList<ApproverStepInput> ApproverChain);
