using Microsoft.AspNetCore.DataProtection;
using Monolith.WebAdmin.Core.Auth;

namespace Monolith.WebAdmin.Services.Storage;

public class TokenCookieStorage(
    IHttpContextAccessor httpContextAccessor,
    IDataProtectionProvider dataProtectionProvider)
    : TokenStorage
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("jwt");

    private const string Key = "client";

    public override Task<UserTokenData?> GetAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = httpContext.Request.Cookies[Key];

            if (!string.IsNullOrEmpty(protectedValue))
            {
                var data = UserTokenData.Read(_protector.Unprotect(protectedValue));

                return Task.FromResult(data);
            }
        }

        return Task.FromResult<UserTokenData?>(default);
    }

    public override Task SaveAsync(UserTokenData token)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = _protector.Protect(token.ToString());

            httpContext.Response.Cookies.Append(Key, protectedValue, new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn),
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
