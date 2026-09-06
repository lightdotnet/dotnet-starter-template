namespace StarterKit.Approval.Contracts.Authorization;

public static class ApprovalPermissions
{
    public const string Group = "approval";

    public static class Requests
    {
        public const string ViewAll = $"{Group}.requests.view_all";
    }
}
