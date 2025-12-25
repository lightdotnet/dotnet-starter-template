using Monolith.Blazor.Services;

namespace Monolith.Blazor.Infrastructure;

public interface ITokenManager
{
    Task<TokenModel?> GetSavedTokenAsync();

    Task<Result<string>> RequestTokenAsync(string username, string password);

    Task<Result<string>> RefreshTokenAsync(string accessToken, string refreshToken);

    Task ClearAsync();
}
