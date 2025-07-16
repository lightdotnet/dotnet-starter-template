﻿using System.Text.Json;

namespace BlazorApp.Core.Auth;

public class SavedToken
{
    public SavedToken()
    { }

    public SavedToken(string accessToken, long expireInSeconds, string? refreshToken)
    {
        Token = accessToken;
        ExpireOn = DateTimeOffset.Now.AddSeconds(expireInSeconds).AddMinutes(-5);

        if (refreshToken != null)
        {
            RefreshToken = refreshToken;
            RefreshTokenExpireOn = DateTimeOffset.Now.AddDays(7);
        }

        //ExpireOn = DateTime.UtcNow.AddSeconds(15);
        //RefreshTokenExpireOn = DateTime.UtcNow.AddSeconds(30);
    }

    public string Token { get; set; } = "";

    public DateTimeOffset ExpireOn { get; set; }

    public string? RefreshToken { get; set; } = "";

    public DateTimeOffset? RefreshTokenExpireOn { get; set; }

    public List<SavedClaim> Claims { get; set; } = [];

    public bool IsExpired() => ExpireOn <= DateTime.UtcNow;

    public bool IsRefreshTokenExpired() => RefreshTokenExpireOn <= DateTime.UtcNow;

    public override string ToString()
    {
        var json = JsonSerializer.Serialize(this);
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static SavedToken? ReadFrom(string? base64EncodedData)
    {
        if (string.IsNullOrEmpty(base64EncodedData)) return null;

        var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
        var data = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        return JsonSerializer.Deserialize<SavedToken>(data);
    }
}
