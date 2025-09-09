using Microsoft.AspNetCore.Authentication;
using Monolith.HttpApi.Identity;
using Monolith.WebAdmin.Core.Auth;
using System.Security.Claims;

namespace Monolith.WebAdmin.Services;

public interface IAuthService
{
    Task<Result> LoginAsync(string username, string password, bool remember);

    Task LogoutAsync();
}

public class AuthService(
    TokenStorage tokenStorage,
    IHttpContextAccessor httpContextAccessor) : IAuthService
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext
        ?? throw new ArgumentNullException("HttpContext is null");

    public async Task<Result> LoginAsync(string username, string password, bool remember)
    {
        var tokenService = _httpContext.RequestServices.GetRequiredService<TokenHttpService>();

        var getToken = await tokenService.GetTokenAsync(username, password);

        if (getToken.Succeeded is false)
        {
            return Result.Error("Invalid username or password.");
        }

        var accessToken = getToken.Data.AccessToken;

        var tokenData = new UserTokenData(accessToken, getToken.Data.ExpiresIn);

        var userClaims = JwtReader.ReadClaims(accessToken);

        var claimsIdentity = new ClaimsIdentity(userClaims, "jwt");

        // Replace with new ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Build authentication properties (optional)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = remember,  // "Remember me"
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(tokenData.ExpiresIn)
        };

        // Sign in using Identity's scheme
        await _httpContext.SignInAsync(claimsPrincipal, authProperties);

        // Store the token securely (e.g., in session or secure storage)
        await tokenStorage.SaveAsync(tokenData);

        return Result.Success();
    }

    public Task LogoutAsync()
    {
        return Task.WhenAll(
            _httpContext.SignOutAsync(),
            tokenStorage.ClearAsync()
        );
    }
}
