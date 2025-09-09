namespace Monolith.WebAdmin.Services.Storage;

/*
public class TokenLocalStorage(ProtectedLocalStorage storage)
{
    private const string Key = "client";

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
*/