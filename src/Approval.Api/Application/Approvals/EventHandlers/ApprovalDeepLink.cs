namespace StarterKit.Approval.Api.Application.Approvals.EventHandlers;

/// <summary>
/// Client route an approval notification deep-links to when the caller supplied no
/// explicit <c>DeepLinkUrl</c>. Kept in one place so the two event handlers can't drift.
/// </summary>
internal static class ApprovalDeepLink
{
    public static string RequestDetail(string approvalRequestId) =>
        $"/approvals/requests/{approvalRequestId}";
}
