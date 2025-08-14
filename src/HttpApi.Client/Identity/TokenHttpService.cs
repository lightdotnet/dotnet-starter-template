using Light.Identity;

namespace Monolith.HttpApi.Identity;

public class TokenHttpService(IHttpClientFactory httpClientFactory)
    : TryHttpClient(httpClientFactory)
{
    private const string BaseUrl = "oauth/token";

    public Task<Result<TokenDto>> GetTokenAsync(string username, string password)
    {
        var url = $"{BaseUrl}/get";

        var request = new { username, password };

        return TryPostAsync<TokenDto>(url, request);
    }

    public Task<Result<TokenDto>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var url = $"{BaseUrl}/refresh";

        var request = new { accessToken, refreshToken };

        return TryPostAsync<TokenDto>(url, request);
    }
}
