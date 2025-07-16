namespace Monolith;

public interface ITokenProvider
{
    //Task<string?> AccessToken { get; }

    Task<string?> GetAccessTokenAsync();
}
