namespace Monolith.Blazor.Services.Token;

public class TokenCookieStorage(IHttpContextAccessor httpContextAccessor) : TokenStorage
{
    public override Task<TokenModel?> GetAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            var tokenCookieValue = httpContext.Request.Cookies[Constants.TokenCookieName];

            var tokenData = TokenModel.ReadFrom(tokenCookieValue);

            Console.WriteLine("Token loaded from Cookies");

            return Task.FromResult(tokenData);;
        }

        return Task.FromResult<TokenModel?>(default);
    }

    public override Task SaveAsync(TokenModel token)
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Response.Cookies.Append(
                Constants.TokenCookieName,
                token.ToString(),
                new CookieOptions
                {
                    Expires = token.ExpireOn,
                    SameSite = SameSiteMode.Strict,
                    Secure = httpContext.Request.IsHttps,
                    HttpOnly = true
                });
        }

        return Task.CompletedTask;
    }

    public override Task ClearAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            httpContext.Response.Cookies.Delete(Constants.TokenCookieName);
        }

        return Task.CompletedTask;
    }
}
