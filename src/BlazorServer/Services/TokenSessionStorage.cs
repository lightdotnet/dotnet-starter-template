using Monolith.BlazorServer.Core.Auth;

namespace Monolith.BlazorServer.Services;

public class TokenSessionStorage(IHttpContextAccessor httpContextAccessor)
    : TokenStorage
{
    private const string AccessTokenCookieName = "AccessToken";

    public override Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            return Task.FromResult(httpContext.Session.GetString("jwt"));
        }

        return Task.FromResult<string?>(default);
    }

    public override Task SetAccessTokenAsync(string accessToken)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Session.SetString("jwt", accessToken);
        }

        return Task.CompletedTask;
    }

    public override Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

        return Task.CompletedTask;
    }
}
