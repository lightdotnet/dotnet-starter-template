namespace StarterKit.Approval.Contracts.Approvals;

/// <summary>
/// One level of an approval chain, already resolved by the calling module (Approval never
/// resolves approvers itself — see <see cref="CreateApprovalRequest"/>).
/// </summary>
public record ApproverStepInput(int Level, string ApproverUserId, string ApproverEmployeeId);
