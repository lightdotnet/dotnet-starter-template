using Light.Blazor;
using Monolith.HttpApi.Identity;

namespace Monolith.Blazor.Services;

public class TokenManager(
    IStorageService storageService,
    TokenHttpService tokenService) : ITokenManager
{
    private const string FIRST_TOKEN_PART = "_fp";

    private const string MIDDLE_TOKEN_PART = "_mp";

    private const string LAST_TOKEN_PART = "_lp";

    private const string TOKEN_LIFETIME = "_exp";

    private const string REFRESH_TOKEN = "refresh_token";

    private const string REFRESH_TOKEN_LIFETIME = "refresh_exp";

    public async Task<SavedToken?> GetSavedTokenAsync()
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

    private async Task<SavedToken?> TryGetTokenCachedAsync()
    {
        try
        {
            var firstPart = await storageService.GetAsync<string>(FIRST_TOKEN_PART);
            var middlePart = await storageService.GetAsync<string>(MIDDLE_TOKEN_PART);
            var lastPart = await storageService.GetAsync<string>(LAST_TOKEN_PART);
            var tokenExp = await storageService.GetAsync<DateTimeOffset?>(TOKEN_LIFETIME);

            if (firstPart is null || middlePart is null || lastPart is null || tokenExp is null)
                return default;

            var accessToken = string.Join('.', firstPart, middlePart, lastPart);

            var refreshToken = await storageService.GetAsync<string>(REFRESH_TOKEN);
            var refreshTokenExp = await storageService.GetAsync<DateTimeOffset?>(REFRESH_TOKEN_LIFETIME);

            return new SavedToken
            {
                Token = accessToken,
                ExpireOn = tokenExp.Value,
                RefreshToken = refreshToken,
                RefreshTokenExpireOn = refreshTokenExp
            };
        }
        catch
        {

        }

        return null;
    }

    private async Task SetTokenAsync(SavedToken data)
    {
        var tokenParts = data.Token.Split('.');

        await storageService.SetAsync(FIRST_TOKEN_PART, tokenParts[0]);
        await storageService.SetAsync(MIDDLE_TOKEN_PART, tokenParts[1]);
        await storageService.SetAsync(LAST_TOKEN_PART, tokenParts[2]);
        await storageService.SetAsync(TOKEN_LIFETIME, data.ExpireOn);

        if (!string.IsNullOrEmpty(data.RefreshToken))
        {
            await storageService.SetAsync(REFRESH_TOKEN, data.RefreshToken);
            await storageService.SetAsync(REFRESH_TOKEN_LIFETIME, data.RefreshTokenExpireOn);
        }
    }

    public async Task<Result<string>> RequestTokenAsync(string username, string password)
    {
        var getToken = await tokenService.GetTokenAsync(username, password);

        if (getToken.Succeeded is false)
        {
            return Result<string>.Error(getToken.Message);
        }

        await SetTokenAsync(
            new SavedToken(
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
            new SavedToken(
                refresh.Data.AccessToken,
                refresh.Data.ExpiresIn,
                refresh.Data.RefreshToken));

        return Result<string>.Success(refresh.Data.AccessToken);
    }

    public async Task ClearAsync() => await storageService.ClearAsync();
}