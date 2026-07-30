using StarterKit.Identity.Contracts;

namespace StarterKit.Identity.Api.Jwt;

public interface ITokenService
{
    Task<IResult<TokenDto>> GetTokenAsync(
        string username, string password,
        DeviceDto? device = null);

    Task<IResult<TokenDto>> RefreshTokenAsync(
        string accessToken, string refreshToken,
        DeviceDto? device = null);
}
