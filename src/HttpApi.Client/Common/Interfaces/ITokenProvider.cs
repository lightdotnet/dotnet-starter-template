namespace Monolith.HttpApi.Common.Interfaces;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync();

    Task SetAccessTokenAsync(string accessToken);

    Task ClearAsync();
}
