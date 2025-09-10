using Monolith.Core.Auth;
using Light.Contracts;
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
