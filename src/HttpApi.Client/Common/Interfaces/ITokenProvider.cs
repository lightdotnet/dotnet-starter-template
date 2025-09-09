namespace Monolith.HttpApi.Common.Interfaces;

public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync();
}
