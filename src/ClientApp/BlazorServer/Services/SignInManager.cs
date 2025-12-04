using Light.Contracts;
using Microsoft.AspNetCore.Authentication;
using Monolith.Blazor.Extensions;
using Monolith.HttpApi.Identity;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class SignInManager(IHttpContextAccessor httpContextAccessor) : ISignInManager
{
    private readonly HttpContext httpContext = httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("HTTP context is not available.");

    public async Task<Result> SignInAsync(LoginModel model)
    {
        var tokenService = httpContext.RequestServices.GetRequiredService<TokenHttpService>();

        var getToken = await tokenService.GetTokenAsync(model.Username, model.Password);

        if (getToken.Succeeded is false)
        {
            return Result.Error("Invalid username or password.");
        }

        var accessToken = getToken.Data.AccessToken;

        //var tokenData = new UserTokenData(accessToken, getToken.Data.ExpiresIn);

        var userClaims = JwtExtensions.ReadClaims(accessToken);

        var claimsIdentity = new ClaimsIdentity(userClaims, "jwt");

        // Replace with new ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Build authentication properties (optional)
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,  // "Remember me"
            ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(getToken.Data.ExpiresIn)
        };

        // Sign in using Identity's scheme
        await httpContext.SignInAsync("jwt", claimsPrincipal, authProperties);

        // Store the token securely (e.g., in session or secure storage)
        //await tokenStorage.SaveAsync(tokenData);

        return Result.Success();
    }

    public Task SignOutAsync()
    {
        return Task.WhenAll(
            httpContext.SignOutAsync()
            //tokenStorage.ClearAsync()
        );
    }
}
