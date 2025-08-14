using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.BlazorServer.Services;

public class TokenProvider(ProtectedLocalStorage storage)
    : ITokenProvider
{
    private const string AccessTokenCookieName = "AccessToken";

    private const string Key = "jwt";

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var token = await storage.GetAsync<string>(Key);

            return token.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
    }

    public async Task SetAccessTokenAsync(string accessToken)
    {
        await storage.SetAsync(Key, accessToken);
    }

    public async Task ClearAsync()
    {
        await storage.DeleteAsync(Key);
    }
}
