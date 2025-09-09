namespace Monolith.Auth;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);