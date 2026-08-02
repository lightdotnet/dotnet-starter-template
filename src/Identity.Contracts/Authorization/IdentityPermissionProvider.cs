using Light.AspNetCore.Authorization;

namespace StarterKit.Identity.Contracts.Authorization;

public class IdentityPermissionProvider : IPermissionDefinitionProvider
{
    public IEnumerable<PermissionDefinition> Define()
    {
        yield return new(
            IdentityPermissions.Roles.View,
            "View Roles");

        yield return new(
            IdentityPermissions.Roles.Manage,
            "Manage Roles");

        yield return new(
            IdentityPermissions.Users.View,
            "View Users");

        yield return new(
            IdentityPermissions.Users.Create,
            "Create Users");

        yield return new(
            IdentityPermissions.Users.Update,
            "Update Users");

        yield return new(
            IdentityPermissions.Users.Delete,
            "Delete Users");
    }
}
