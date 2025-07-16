﻿using BlazorApp.Core;
using BlazorApp.Core.Auth;
using Light.Contracts;
using Monolith.Identity;
using System.Security.Claims;

namespace BlazorApp.Infrastructure;

public class AppTokenService(
    IStorageService storageService,
    TokenHttpService tokenService) : IAppTokenService
{
    private const string identityCacheKey = "identity";

    public async Task<SavedToken?> GetSavedTokenAsync()
    {
        var savedToken = await TryGetTokenCachedAsync();

        if (savedToken is not null
            && savedToken.IsExpired()
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
            var dataAsString = await storageService.GetAsync<string>(identityCacheKey);

            return SavedToken.ReadFrom(dataAsString);
        }
        catch
        {

        }

        return null;
    }

    private async Task SetTokenAsync(SavedToken token)
    {
        var tokenClaims = JwtExtensions.ReadClaims(token.Token)
            .Select(s => new SavedClaim(s.Type, s.Value));

        token.Claims.AddRange(tokenClaims);

        // save token first for other handler api services to use
        await storageService.SetAsync(identityCacheKey, token.ToString());
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

    public async Task<IEnumerable<Claim>?> GetUserClaimsAsync()
    {
        var values = await TryGetTokenCachedAsync();

        return values?.Claims.Select(s => new Claim(s.Type, s.Value));
    }
}