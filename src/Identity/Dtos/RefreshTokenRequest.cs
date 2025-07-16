namespace Monolith.Identity.Dtos;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);