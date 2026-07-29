using Light.Domain;
using Light.Domain.Entities;

namespace StarterKit.Identity.Entities;

public class JwtToken : Entity
{
    public JwtToken() => Id = LightId.NewId();

    public required string UserId { get; set; }

    public string? Token { get; set; }

    public DateTimeOffset TokenExpiresAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpiresAt { get; set; }

    public bool Revoked { get; set; }

    public string? DeviceId { get; set; }

    public string? DeviceName { get; set; }

    public string? IpAddress { get; set; }

    public string? PhysicalAddress { get; set; }

    public long TokenExpiresInSeconds => (long)(TokenExpiresAt - DateTimeOffset.Now).TotalSeconds;
}
