namespace Monolith.WebAdmin.Core.Auth;

public abstract class TokenStorage
{
    public abstract Task<string?> GetAccessTokenAsync();

    public abstract Task SetAccessTokenAsync(string accessToken);

    public abstract Task ClearAsync();
}
