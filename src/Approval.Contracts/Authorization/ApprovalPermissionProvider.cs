using Light.AspNetCore.Authorization;

namespace StarterKit.Approval.Contracts.Authorization;

public class ApprovalPermissionProvider : IPermissionDefinitionProvider
{
    public IEnumerable<PermissionDefinition> Define()
    {
        yield return new(
            ApprovalPermissions.Requests.ViewAll,
            "View All Approval Requests",
            ApprovalPermissions.Group);
    }
}
