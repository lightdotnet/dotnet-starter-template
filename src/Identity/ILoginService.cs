namespace Monolith.Identity;

public interface ILoginService
{
    Task<IResult<TokenDto>> GetTokenAsync(
        string username, string password,
        string? deviceId = null, string? deviceName = null);

    Task<IResult<TokenDto>> RefreshTokenAsync(string accessToken, string refreshToken);
}
