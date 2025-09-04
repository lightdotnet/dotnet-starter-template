using Microsoft.AspNetCore.Authentication;
using Monolith.WebAdmin.Core.Auth;
using Monolith.HttpApi.Identity;
using System.Security.Claims;

namespace Monolith.WebAdmin.Services;

public interface IAuthService
{
    Task<Result> LoginAsync(string username, string password, bool remember);

    Task LogoutAsync();
}

public class AuthService(
    TokenHttpService tokenHttpService,
    TokenStorage tokenStorage,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<Result> LoginAsync(string username, string password, bool remember)
    {
        var getToken = await tokenHttpService.GetTokenAsync(username, password);

        if (getToken.Succeeded is false)
        {
            return Result.Error("Invalid username or password.");
        }

        var accessToken = getToken.Data.AccessToken;

        // Store the token securely (e.g., in session or secure storage)
        await tokenStorage.SetAccessTokenAsync(accessToken);

        var userClaims = JwtReader.ReadClaims(accessToken);

        var claimsIdentity = new ClaimsIdentity(userClaims, "jwt");

        // Replace with new ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Build authentication properties (optional)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = remember,  // "Remember me"
            //ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        // Sign in using Identity's scheme
        await httpContextAccessor.HttpContext!
            .SignInAsync(claimsPrincipal, authProperties);

        return Result.Success();
    }

    public Task LogoutAsync()
    {
        return Task.WhenAll(
            httpContextAccessor.HttpContext!.SignOutAsync(),
            tokenStorage.ClearAsync()
        );
    }
}
