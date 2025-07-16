using Light.Identity;
using System.Net.Http.Json;

namespace Monolith.Identity;

public class TokenHttpService(IHttpClientFactory httpClientFactory) :
    HttpClientBase(httpClientFactory)
{
    public async Task<Result<TokenDto>> GetTokenAsync(string username, string password)
    {
        var url = "api/v1/oauth/token/get";

        var request = new { username, password };

        var res = await HttpClient.PostAsJsonAsync(url, request);

        return await res.ReadResultAsync<TokenDto>();
    }

    public async Task<Result<TokenDto>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var url = "api/v1/oauth/token/refresh";

        var request = new { accessToken, refreshToken };

        var res = await HttpClient.PostAsJsonAsync(url, request);

        return await res.ReadResultAsync<TokenDto>();
    }
}