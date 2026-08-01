using StarterKit.Identity.Api.Entities;
using StarterKit.Identity.Contracts;

namespace StarterKit.Identity.Api.Jwt;

public interface IUserSessionService
{
    Task<TokenDto> GenerateTokenAsync(
        User user,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        DeviceDto? device = null);

    Task<TokenDto> RefreshTokenAsync(
        User user,
        string refreshToken,
        string issuer, string secretKey,
        DateTime tokenExpiresAt, DateTime refreshTokenExpiresAt,
        DeviceDto? device = null);

    Task<IEnumerable<UserSessionDto>> GetUserTokensAsync(string userId);

    Task<bool> IsTokenValidAsync(string accessToken);

    Task RevokeAsync(string userId, string tokenId);
}
