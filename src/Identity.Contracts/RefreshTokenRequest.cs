namespace StarterKit.Identity.Contracts;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);