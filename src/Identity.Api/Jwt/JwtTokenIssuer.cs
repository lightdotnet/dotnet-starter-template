using Microsoft.AspNetCore.Identity;
using StarterKit.Identity.Api.Entities;
using StarterKit.Shared.Constants;
using System.Security.Claims;

namespace StarterKit.Identity.Api.Jwt;

internal class JwtTokenIssuer(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
{
    public virtual async Task<string> IssueAsync(
        User user, string tokenId,
        string issuer, string secretKey,
        DateTime tokenExpiresAt)
    {
        var claims = await GetUserClaimsAsync(user);

        claims.Add(new Claim(ClaimTypeConstants.TokenId, tokenId));

        return JwtHelper.GenerateToken(
            issuer,
            claims,
            tokenExpiresAt,
            secretKey);
    }

    private async Task<IList<Claim>> GetUserClaimsAsync(User user)
    {
        var userRoles = await userManager.GetRolesAsync(user);

        var permissionClaims = new List<Claim>();

        foreach (var userRole in userRoles)
        {
            var role = await roleManager.FindByNameAsync(userRole);
            if (role is null)
                continue;

            var roleClaims = await roleManager.GetClaimsAsync(role);

            permissionClaims.AddRange(roleClaims);
        }

        var claims = new List<Claim>
        {
            { ClaimTypeConstants.UserId, user.Id },
            { ClaimTypeConstants.UserName, user.UserName },
        }
        .Union(permissionClaims)
        .Where(x => !string.IsNullOrEmpty(x.Value))
        .ToList();

        return claims;
    }
}
