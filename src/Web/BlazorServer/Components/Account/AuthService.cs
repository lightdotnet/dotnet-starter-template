using Light.Identity.EntityFrameworkCore;
using Light.Identity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Monolith.Components.Account;

public interface IAuthService
{
    Task<Result> LoginAsync(string username, string password, bool remember);

    Task LogoutAsync();
}

public class AuthService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    SignInManager<User> signInManager) : IAuthService
{
    public async Task<Result> LoginAsync(string username, string password, bool remember)
    {
        var user = await userManager.FindByNameAsync(username);

        if (user == null)
        {
            return Result.Error("Invalid username or password.");
        }

        // Start with the default identity (includes NameIdentifier, Name, roles if configured)
        var userPrincipal = await signInManager.CreateUserPrincipalAsync(user);

        var identity = (ClaimsIdentity)userPrincipal.Identity!;

        var userClaims = await new UserClaimProvider(userManager, roleManager).GetUserClaimsAsync(user);

        identity.AddClaims(userClaims);

        // Replace with new ClaimsPrincipal
        var principal = new ClaimsPrincipal(identity);

        // Build authentication properties (optional)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = remember,  // "Remember me"
            //ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Sign in using Identity's scheme
        await signInManager.Context.SignInAsync(principal, authProperties);


        return Result.Success();
    }

    public Task LogoutAsync() => signInManager.Context.SignOutAsync();
}
