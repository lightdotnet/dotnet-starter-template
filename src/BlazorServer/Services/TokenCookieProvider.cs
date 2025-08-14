using Microsoft.AspNetCore.DataProtection;
using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.BlazorServer.Services;

public class TokenCookieProvider(
    IHttpContextAccessor httpContextAccessor,
    IDataProtectionProvider dataProtectionProvider)
    : ITokenProvider
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("jwt");

    public Task<string?> GetAccessTokenAsync()
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

    public Task SetAccessTokenAsync(string accessToken)
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

    public Task ClearAsync()
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
