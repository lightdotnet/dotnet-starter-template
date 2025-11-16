using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Monolith.Infrastructure.Storage;

public class TokenLocalStorage(ProtectedLocalStorage storage) : TokenStorage
{
    private const string Key = "client";

    public override async Task<UserTokenData?> GetAsync()
    {
        try
        {
            var token = await storage.GetAsync<UserTokenData>(Key);

            return token.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
    }

    public override async Task SaveAsync(UserTokenData token)
    {
        await storage.SetAsync(Key, token);
    }

    public override async Task ClearAsync()
    {
        await storage.DeleteAsync(Key);
    }
}