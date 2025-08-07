using Microsoft.AspNetCore.Authorization;

namespace Monolith;

public record PermissionRequirement(string Permission) : IAuthorizationRequirement;