namespace Monolith.Identity.Jwt;

public interface ITokenService
{
    Task<IResult<TokenDto>> GetTokenAsync(
        string username, string password,
        string? deviceId = null, string? deviceName = null);

    Task<IResult<TokenDto>> RefreshTokenAsync(string accessToken, string refreshToken);
}
