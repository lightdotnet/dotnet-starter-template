using Light.Blazor;
using Light.Contracts;
using Monolith.Blazor.Services;
using Monolith.HttpApi.Identity;

namespace Monolith.Blazor.Infrastructure;

public class TokenManager(
    IStorageService storageService,
    TokenHttpService tokenService) : ITokenManager
{
    private const string TOKEN_CACHE_KEY = "_user_session";

    public async Task<TokenModel?> GetSavedTokenAsync()
    {
        var savedToken = await TryGetTokenCachedAsync();

        if (savedToken is not null
            && savedToken.IsNearlyExpired()
            && savedToken.RefreshToken is not null)
        {
            if (savedToken.IsRefreshTokenExpired() is false)
            {
                //var refreshToken = Result.Error();
                var refreshToken = await RefreshTokenAsync(savedToken.Token, savedToken.RefreshToken);

                if (refreshToken.Succeeded)
                {
                    return await TryGetTokenCachedAsync();
                }

                Console.WriteLine($"Refresh token error: {refreshToken.Message}");
            }

            await ClearAsync();
            return null;
        }

        return savedToken;
    }

    private async Task<TokenModel?> TryGetTokenCachedAsync()
    {
        try
        {
            var dataAsString = await storageService.GetAsync<string>(TOKEN_CACHE_KEY);

            return TokenModel.ReadFrom(dataAsString);
        }
        catch { }

        return null;
    }

    private async Task SetTokenAsync(TokenModel data)
    {
        await storageService.SetAsync(TOKEN_CACHE_KEY, data.ToString());
    }

    public async Task<Result<string>> RequestTokenAsync(string username, string password)
    {
        var getToken = await tokenService.GetTokenAsync(username, password);

        if (getToken.Succeeded is false)
        {
            return Result<string>.Error(getToken.Message);
        }

        await SetTokenAsync(
            new TokenModel(
                getToken.Data.AccessToken,
                getToken.Data.ExpiresIn,
                getToken.Data.RefreshToken));

        return Result<string>.Success(getToken.Data.AccessToken);
    }

    public async Task<Result<string>> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var refresh = await tokenService.RefreshTokenAsync(accessToken, refreshToken);

        if (refresh.Succeeded is false)
        {
            return Result<string>.Error(refresh.Message);
        }

        await SetTokenAsync(
            new TokenModel(
                refresh.Data.AccessToken,
                refresh.Data.ExpiresIn,
                refresh.Data.RefreshToken));

        return Result<string>.Success(refresh.Data.AccessToken);
    }

    public async Task ClearAsync() => await storageService.ClearAsync();
}