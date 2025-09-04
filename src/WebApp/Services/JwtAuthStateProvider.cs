using Microsoft.AspNetCore.Components.Authorization;
using Monolith.BlazorServer.Core.Auth;
using Monolith.HttpApi.Common.Interfaces;
using System.Security.Claims;

namespace Monolith.BlazorServer.Services;

public class JwtAuthStateProvider(
    IHttpContextAccessor httpContextAccessor,
    TokenStorage tokenStorage)
    : AuthenticationStateProvider, ITokenProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public ClaimsPrincipal? CurrentUser { get; private set; }

    public Task<string?> GetAccessTokenAsync() => tokenStorage.GetAccessTokenAsync();

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = httpContextAccessor.HttpContext?.User;

        user ??= _anonymous;

        return Task.FromResult(new AuthenticationState(user));
    }
}
