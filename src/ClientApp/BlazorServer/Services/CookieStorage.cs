using Light.Blazor;
using Microsoft.AspNetCore.DataProtection;
using Monolith.Blazor.Services.Token;

namespace Monolith.Blazor.Services;

public class CookieStorage(
    IHttpContextAccessor httpContextAccessor,
    IDataProtectionProvider dataProtectionProvider)
    : IStorageService
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("jwt");

    public ValueTask<T?> GetAsync<T>(string key)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var protectedValue = httpContext.Request.Cookies[key];

            if (!string.IsNullOrEmpty(protectedValue))
            {
                var unprotectedData = _protector.Unprotect(protectedValue);

                var obj = System.Text.Json.JsonSerializer.Deserialize<T>(unprotectedData);

                return new ValueTask<T?>(obj);
            }
        }

        return new ValueTask<T?>();
    }

    public ValueTask SetAsync<T>(string key, T data)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(data);

            var protectedValue = _protector.Protect(json);

            httpContext.Response.Cookies.Append(key, protectedValue);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask RemoveAsync(string key)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(key);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            HttpContextEntensions.ClearCookies(httpContext);
        }

        return ValueTask.CompletedTask;
    }
}
