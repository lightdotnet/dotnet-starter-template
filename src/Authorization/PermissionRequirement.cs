using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; private set; } = permission;
}