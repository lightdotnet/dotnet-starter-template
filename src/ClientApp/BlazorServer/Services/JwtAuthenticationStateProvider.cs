using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Blazor.Extensions;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class JwtAuthenticationStateProviderServer(ITokenProvider tokenProvider) : AuthenticationStateProvider
{
    public ClaimsPrincipal? CurrentUser { get; private set; }

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
        var accessToken = await tokenProvider.GetAccessTokenAsync();

        if (string.IsNullOrEmpty(accessToken))
        {
            return default;
        }

        var userClaims = JwtExtensions.ReadClaims(accessToken);

        // must set authenticationType to mark isAuthentitcated = true
        var identity = new ClaimsIdentity(userClaims, "jwt");

        Console.WriteLine("User loaded");

        return new ClaimsPrincipal(identity);
    }
}
