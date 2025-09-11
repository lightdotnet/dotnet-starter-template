using Light.Contracts;
using Monolith.Core.Auth;
using System.Security.Claims;

namespace Monolith.Core;

public interface IAppTokenService
{
    Task<SavedToken?> GetSavedTokenAsync();

    Task<Result<string>> RequestTokenAsync(string username, string password);

    Task<Result<string>> RefreshTokenAsync(string accessToken, string refreshToken);

    Task ClearAsync();

    Task<IEnumerable<Claim>?> GetUserClaimsAsync();
}
