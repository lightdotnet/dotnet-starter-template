using Light.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Core;
using Monolith.Core.Extensions;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Infrastructure;

public class JwtAuthenticationStateProvider(
    IAppTokenService tokenService,
    NavigationManager navigationManager) :
    AuthenticationStateProvider, IIdentityManager, ITokenProvider
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ClaimsPrincipal? CurrentUser { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUser = await GetUserClaimsPrincipalAsync();

        return new AuthenticationState(CurrentUser);
    }

    private async Task<ClaimsPrincipal> GetUserClaimsPrincipalAsync()
    {
        var userClaims = await tokenService.GetUserClaimsAsync();

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
        var savedToken = await tokenService.GetSavedTokenAsync();

        if (savedToken == null)
        {
            Console.WriteLine("Token is null");

            await LogoutAsync();

            navigationManager.RedirectToLogin();

            return default;
        }

        return savedToken.Token;
    }
}
