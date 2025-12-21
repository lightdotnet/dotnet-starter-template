using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Monolith.Blazor.Services.Token;

public class TokenLocalStorage(ProtectedLocalStorage storage) : TokenStorage
{
    private const string Key = "client";

    public override async Task<TokenModel?> GetAsync()
    {
        try
        {
            var token = await storage.GetAsync<TokenModel>(Key);

            return token.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return default;
        }
    }

    public override async Task SaveAsync(TokenModel token)
    {
        await storage.SetAsync(Key, token);
    }

    public override async Task ClearAsync()
    {
        await storage.DeleteAsync(Key);
    }
}