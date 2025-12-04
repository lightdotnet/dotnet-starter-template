using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Blazor.Extensions;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class JwtAuthenticationStateProvider(
    ITokenManager tokenService,
    NavigationManager navigationManager) :
    AuthenticationStateProvider, ITokenProvider, ISignInManager
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ClaimsPrincipal? CurrentUser { get; private set; }

    private SavedToken? TokenData { get; set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUser = await GetUserClaimsPrincipalAsync();

        return new AuthenticationState(CurrentUser);
    }

    private async Task<ClaimsPrincipal> GetUserClaimsPrincipalAsync()
    {
        var accessToken = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(accessToken))
        {
            return new(new ClaimsIdentity());
        }

        var userClaims = JwtExtensions.ReadClaims(accessToken);

        // must set authenticationType to mark isAuthentitcated = true
        var identity = new ClaimsIdentity(userClaims, "jwt");

        return new ClaimsPrincipal(identity);
    }

    public async Task<Result> SignInAsync(LoginModel model)
    {
        // We make sure the access token is only refreshed by one thread at a time. The other ones have to wait.
        await _semaphore.WaitAsync();

        try
        {
            var getToken = await tokenService.RequestTokenAsync(model.Username, model.Password);

            if (getToken.Succeeded)
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }

            return getToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task SignOutAsync()
    {
        await _semaphore.WaitAsync();

        try
        {
            await tokenService.ClearAsync();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            navigationManager.RedirectToLogin(true);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwMUtBNTlOTUYxMVIyV1lCV1NSU1FNR1E2ViIsInVuIjoic3VwZXIiLCJ0aWQiOiIwMUtCS1pKVjZGMVpCVEYxSkhCUlA2MEFGRiIsImV4cCI6MTc2NDgzMjA3NCwiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3QifQ.EmndFWxvY2V5Y9Udq9_EF6HR-4WwEm_jqfMG0KSxJds";


        TokenData ??= await tokenService.GetSavedTokenAsync();

        if (TokenData is null)
        {
            Console.WriteLine("Token is null");

            //await SignOutAsync();

            return default;
        }

        return TokenData.Token;
    }
}
