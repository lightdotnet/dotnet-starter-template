using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Blazor.Services;

public class TokenProvider(IHttpContextAccessor httpContextAccessor) : ITokenProvider
{
    public async Task<string?> GetAccessTokenAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var tokenCookieValue = httpContext.Request.Cookies[Constants.TokenCookieName];

            var tokenData = TokenModel.ReadFrom(tokenCookieValue);

            return tokenData?.Token;
        }

        return default;
    }
}
