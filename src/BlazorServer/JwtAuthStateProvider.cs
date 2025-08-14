using Light.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Monolith.BlazorServer.Core.Auth;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.HttpApi.Identity;
using System.Security.Claims;

namespace Monolith.BlazorServer;

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

    public async Task<Result> LoginAsync(string username, string password, bool rememberMe)
    {
        if (httpContextAccessor.HttpContext is HttpContext context)
        {
            var tokenService = context.RequestServices.GetRequiredService<TokenHttpService>();

            var getToken = await tokenService.GetTokenAsync(username, password);

            if (getToken.Succeeded)
            {
                var accessToken = getToken.Data.AccessToken;

                await tokenStorage.SetAccessTokenAsync(accessToken);

                var userClaims = JwtReader.ReadClaims(accessToken);
                var claimsIdentity = new ClaimsIdentity(userClaims, "Jwt");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                CurrentUser = claimsPrincipal;

                await context.SignInAsync(
                    CurrentUser,
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.Now.AddSeconds(getToken.Data.ExpiresIn),
                        IsPersistent = rememberMe,
                    });

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CurrentUser)));
            }

            return getToken;
        }

        return Result.Error("HttpContext is not available.");
    }

    public async Task LogoutAsync()
    {
        if (httpContextAccessor.HttpContext is HttpContext context)
        {
            await tokenStorage.ClearAsync();

            await context.SignOutAsync();
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}
