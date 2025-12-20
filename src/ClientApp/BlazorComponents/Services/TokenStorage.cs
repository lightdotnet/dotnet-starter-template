namespace Monolith.Blazor.Services;

public abstract class TokenStorage
{
    public abstract Task<TokenModel?> GetAsync();

    public abstract Task SaveAsync(TokenModel token);

    public abstract Task ClearAsync();
}
