using Light.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
//using ClaimTypes = Light.Identity.ClaimTypes;

namespace Monolith.BlazorServer.Services;

public class AppUserClaimsPrincipalFactory(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, options)
{
    private readonly UserClaimProvider _userClaimProvider = new(userManager, roleManager);

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        /*
        identity.AddClaim(new Claim(ClaimTypes.UserId, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.UserName, user.UserName ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.FirstName, user.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.LastName, user.LastName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.FullName, user.LastName + " " + user.FirstName ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.PhoneNumber, user.PhoneNumber ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? string.Empty));
        return identity;
        */

        var claims = await _userClaimProvider.GetUserClaimsAsync(user);
        //var identity = new ClaimsIdentity(claims, "BlazorApp");

        identity.AddClaims(claims);

        return identity;
    }
}
