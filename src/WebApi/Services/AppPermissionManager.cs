using Monolith.Authorization;
using Monolith.Extensions;
using System.Security.Claims;

namespace Monolith.Services;

public class AppPermissionManager(ILogger<AppPermissionManager> logger)
{
    private readonly string _permissionClaimType = Monolith.Claims.ClaimTypes.Permission;

    private IEnumerable<Claim>? Claims { get; set; }

    private IEnumerable<Claim> GetClaims()
    {
        if (Claims is null)
        {
            var claims = new List<Claim>();
            
            var identityPermissions = ReflectionHelper.GetPublicConstants(typeof(IdentityPermissions));
            claims.AddRange(identityPermissions.Select(x => new Claim(_permissionClaimType, x)));

            logger.LogWarning("AppPermissionManager initialized with {count} claims.", claims.Count);

            Claims = claims;
        }

        return Claims;
    }

    public Task<bool> IsValidAsync(string permission)
    {
        var result = GetClaims()?.Any(x => x.Type == _permissionClaimType && x.Value == permission);

        return Task.FromResult(result is true);
    }
}
