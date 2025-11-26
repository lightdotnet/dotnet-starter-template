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
        var userClaims = new List<Claim>();

        if (userClaims is null)
        {
            return new(new ClaimsIdentity());
        }

        // must set authenticationType to mark isAuthentitcated = true
        var identity = new ClaimsIdentity(userClaims, "JWT");

        return new ClaimsPrincipal(identity);
    }

    public async Task<Result> LoginAsync(string userName, string password)
    {
        // We make sure the access token is only refreshed by one thread at a time. The other ones have to wait.
        await _semaphore.WaitAsync();

        try
        {
            var getToken = await tokenService.RequestTokenAsync(userName, password);

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

    public async Task LogoutAsync()
    {
        // We make sure the access token is only refreshed by one thread at a time. The other ones have to wait.
        await _semaphore.WaitAsync();

        try
        {
            await tokenService.ClearAsync();

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        TokenData ??= await tokenService.GetSavedTokenAsync();

        if (TokenData is null)
        {
            Console.WriteLine("Token is null");

            await LogoutAsync();

            navigationManager.RedirectToLogin();

            return default;
        }

        return TokenData.Token;
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
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
