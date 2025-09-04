using Monolith.BlazorServer.Core.Auth;

namespace Monolith.BlazorServer.Services.Storage;

/// <remarks>
/// Please make sure:
///     AddSession() to IServiceCollection
///     UseSession() to IApplicationBuilder
/// before application start.
/// </remarks>
public class TokenSessionStorage(IHttpContextAccessor httpContextAccessor)
    : TokenStorage
{
    // Using a fixed key for simplicity, consider using a more complex key management strategy for production scenarios.
    private const string Key = "client";

    public override Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            return Task.FromResult(httpContext.Session.GetString(Key));
        }

        return Task.FromResult<string?>(default);
    }

    public override Task SetAccessTokenAsync(string accessToken)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Session.SetString(Key, accessToken);
        }

        return Task.CompletedTask;
    }

    public override Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            HttpContextEntensions.ClearCookies(httpContext);
            httpContext.Session.Clear();
        }

        return Task.CompletedTask;
    }
}
