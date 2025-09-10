using Light.Identity;

namespace Monolith.HttpApi.Identity;

public class UserProfileHttpService(IHttpClientFactory httpClientFactory) :
    TryHttpClient(httpClientFactory)
{
    private const string _path = "user_profile";

    public Task<Result<IEnumerable<UserTokenDto>>> GetTokensAsync()
    {
        var url = $"{_path}/token/list";
        return TryGetAsync<IEnumerable<UserTokenDto>>(url);
    }

    public Task<Result> RevokeTokenAsync(string tokenId)
    {
        var url = $"{_path}/token/revoke";
        return TryPutAsync(url, tokenId);
    }
}