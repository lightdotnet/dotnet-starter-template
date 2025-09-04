using Microsoft.AspNetCore.DataProtection;
using Monolith.BlazorServer.Core.Auth;

namespace Monolith.BlazorServer.Services.Storage;

public class TokenCookieStorage(
    IHttpContextAccessor httpContextAccessor,
    IDataProtectionProvider dataProtectionProvider)
    : TokenStorage
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("jwt");

    private const string Key = "client";

    public override Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = httpContext.Request.Cookies[Key];

            if (!string.IsNullOrEmpty(protectedValue))
            {
                return Task.FromResult<string?>(_protector.Unprotect(protectedValue));
            }
        }

        return Task.FromResult<string?>(default);
    }

    public override Task SetAccessTokenAsync(string accessToken)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = _protector.Protect(accessToken);

            httpContext.Response.Cookies.Append(
                Key,
                protectedValue,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7),
                });
        }

        return Task.CompletedTask;
    }

    public override Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            HttpContextEntensions.ClearCookies(httpContext);
        }

        return Task.CompletedTask;
    }
}
