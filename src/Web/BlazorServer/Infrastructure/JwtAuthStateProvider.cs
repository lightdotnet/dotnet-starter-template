using Microsoft.AspNetCore.Components.Authorization;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.Infrastructure;

public class JwtAuthStateProvider(
    IHttpContextAccessor httpContextAccessor,
    TokenStorage tokenStorage)
    : AuthenticationStateProvider, ITokenProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public ClaimsPrincipal? CurrentUser { get; private set; }

    public async Task<string?> GetAccessTokenAsync()
    {
        var tokenData = await tokenStorage.GetAsync();

        return tokenData?.AccessToken;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;

        user ??= _anonymous;

        return Task.FromResult(new AuthenticationState(user));
    }
}
