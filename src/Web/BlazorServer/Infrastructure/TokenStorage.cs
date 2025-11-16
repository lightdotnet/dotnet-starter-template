namespace Monolith.Infrastructure;

public abstract class TokenStorage
{
    public abstract Task<UserTokenData?> GetAsync();

    public abstract Task SaveAsync(UserTokenData token);

    public abstract Task ClearAsync();
}
