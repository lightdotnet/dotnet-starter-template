using Light.Contracts;
using Microsoft.AspNetCore.Authentication;
using Monolith.Blazor.Extensions;
using Monolith.Blazor.Services.Storage;
using Monolith.HttpApi.Identity;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class SignInManager(
    IHttpContextAccessor httpContextAccessor,
    TokenMemoryStorage tokenStorage) : ISignInManager
{
    private readonly HttpContext _httpContext = httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("HttpContext is not available.");

    public async Task<Result> SignInAsync(LoginModel model)
    {
        var tokenService = _httpContext.RequestServices.GetRequiredService<TokenHttpService>();

        var getToken = await tokenService.GetTokenAsync(model.Username, model.Password);

        if (getToken.Succeeded is false)
        {
            return Result.Error("Invalid username or password.");
        }

        var id = Guid.NewGuid().ToString("N");

        var tokenData = new TokenModel(
            getToken.Data.AccessToken,
            getToken.Data.ExpiresIn,
            getToken.Data.RefreshToken);

        await tokenStorage.SaveAsync(id, tokenData);

        var isHttps = _httpContext.Request.IsHttps;

        _httpContext.Response.Cookies.Append(
            Constants.TokenCookieName,
            tokenData.ToString(),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddSeconds(getToken.Data.ExpiresIn)
            });

        var userClaims = JwtExtensions.ReadClaims(getToken.Data.AccessToken);

        var claimsIdentity = new ClaimsIdentity(userClaims, Constants.JwtAuthScheme);

        // Replace with new ClaimsPrincipal
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Sign in using Identity's scheme
        await _httpContext.SignInAsync(
            Constants.JwtAuthScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,  // "Remember me"
                ExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(getToken.Data.ExpiresIn),
                AllowRefresh = true
            });

        return Result.Success();
    }

    public async Task SignOutAsync()
    {
        HttpContextEntensions.ClearCookies(_httpContext);
        await _httpContext.SignOutAsync();
    }
}
