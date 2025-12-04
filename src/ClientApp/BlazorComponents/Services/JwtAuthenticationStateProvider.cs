using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Blazor.Extensions;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class JwtAuthenticationStateProvider(
    IServiceScopeFactory serviceScopeFactory,
    NavigationManager navigationManager) :
    AuthenticationStateProvider, ITokenProvider, ISignInManager
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ClaimsPrincipal? CurrentUser { get; private set; }

    private SavedToken? TokenData { get; set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUser ??= await GetUserClaimsPrincipalAsync();

        if (CurrentUser is null)
        {
            return new AuthenticationState(new(new ClaimsIdentity()));
        }

        return new AuthenticationState(CurrentUser);
    }

    private async Task<ClaimsPrincipal?> GetUserClaimsPrincipalAsync()
    {
        var accessToken = await GetAccessTokenAsync();

        if (string.IsNullOrEmpty(accessToken))
        {
            return default;
        }

        var userClaims = JwtExtensions.ReadClaims(accessToken);

        // must set authenticationType to mark isAuthentitcated = true
        var identity = new ClaimsIdentity(userClaims, "JWT");

        Console.WriteLine("User loaded");

        return new ClaimsPrincipal(identity);
    }

    public async Task<Result> SignInAsync(LoginModel model)
    {
        // We make sure the access token is only refreshed by one thread at a time. The other ones have to wait.
        await _semaphore.WaitAsync();

        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenManager>();

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
            using var scope = serviceScopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenManager>();

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
        if (TokenData is null)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenManager>();

            TokenData = await tokenService.GetSavedTokenAsync();
        }

        if (TokenData is null)
        {
            Console.WriteLine("Token is null");

            //await SignOutAsync();

            return default;
        }

        return TokenData.Token;
    }
}
