using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Blazor.Extensions;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class JwtAuthenticationStateProviderServer(
    TokenStorage tokenStorage)
    : AuthenticationStateProvider, ITokenProvider
{
    public ClaimsPrincipal? CurrentUser { get; private set; }

    public TokenModel? TokenData { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        CurrentUser ??= await GetUserClaimsPrincipalAsync();

        return CurrentUser is null
            ? new AuthenticationState(new(new ClaimsIdentity()))
            : new AuthenticationState(CurrentUser);
    }

    private async Task<ClaimsPrincipal?> GetUserClaimsPrincipalAsync()
    {
        TokenData ??= await tokenStorage.GetAsync();

        if (TokenData is null)
        {
            return default;
        }

        var userClaims = JwtExtensions.ReadClaims(TokenData.Token);

        // must set authenticationType to mark isAuthentitcated = true
        var identity = new ClaimsIdentity(userClaims, "jwt");

        Console.WriteLine("User loaded");

        return new ClaimsPrincipal(identity);
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        TokenData ??= await tokenStorage.GetAsync();

        return TokenData?.Token;
    }
}
