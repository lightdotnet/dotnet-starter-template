using Microsoft.AspNetCore.DataProtection;
using Monolith.BlazorServer.Core.Auth;

namespace Monolith.BlazorServer.Services;

public class TokenCookieStorage(
    IHttpContextAccessor httpContextAccessor,
    IDataProtectionProvider dataProtectionProvider)
    : TokenStorage
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("jwt");

    public override Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = httpContext.Request.Cookies["jwt"];

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
                "jwt",
                protectedValue,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
        }

        return Task.CompletedTask;
    }

    public override Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            foreach (var cookie in httpContext.Request.Cookies.Keys)
            {
                httpContext.Response.Cookies.Delete(cookie);
            }
        }

        return Task.CompletedTask;
    }
}
