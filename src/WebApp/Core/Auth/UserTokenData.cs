using System.Text.Json;

namespace Monolith.WebAdmin.Core.Auth;

public class UserTokenData
{
    public UserTokenData() { }

    public UserTokenData(string accessToken, long expiresIn)
    {
        AccessToken = accessToken;
        ExpiresIn = expiresIn - 300;
    }

    public string AccessToken { get; set; } = null!;

    public long ExpiresIn { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public static UserTokenData? Read(string json)
    {
        return JsonSerializer.Deserialize<UserTokenData>(json);
    }
}
