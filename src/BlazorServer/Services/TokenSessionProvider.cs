using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.BlazorServer.Services;

public class TokenSessionProvider(IHttpContextAccessor httpContextAccessor)
    : ITokenProvider
{
    private const string AccessTokenCookieName = "AccessToken";

    public Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            return Task.FromResult(httpContext.Session.GetString("jwt"));
        }

        return Task.FromResult<string?>(default);
    }

    public Task SetAccessTokenAsync(string accessToken)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Session.SetString("jwt", accessToken);
        }

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Session.Clear();
        }

        return Task.CompletedTask;
    }
}
