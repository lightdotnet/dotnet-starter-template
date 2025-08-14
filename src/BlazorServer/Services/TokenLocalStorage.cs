using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Monolith.BlazorServer.Core.Auth;

namespace Monolith.BlazorServer.Services;

public class TokenLocalStorage(ProtectedLocalStorage storage) : TokenStorage
{
    private const string AccessTokenCookieName = "AccessToken";

    private const string Key = "jwt";

    public override async Task<string?> GetAccessTokenAsync()
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

    public override async Task SetAccessTokenAsync(string accessToken)
    {
        await storage.SetAsync(Key, accessToken);
    }

    public override async Task ClearAsync()
    {
        await storage.DeleteAsync(Key);
    }
}
