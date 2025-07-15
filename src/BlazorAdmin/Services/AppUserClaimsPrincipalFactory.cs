using Light.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ClaimTypes = Light.Identity.ClaimTypes;

namespace BlazorAdmin.Services;

public class AppUserClaimsPrincipalFactory(UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimTypes.UserId, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.UserName, user.UserName ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.FirstName, user.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.LastName, user.LastName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.FullName, user.LastName + " " + user.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? string.Empty));
        return identity;
    }
}
