namespace Monolith.Identity;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);