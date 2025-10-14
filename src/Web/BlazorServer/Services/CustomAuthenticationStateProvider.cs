using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Monolith.Services;

public class CustomAuthenticationStateProvider(
    IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    public ClaimsPrincipal? CurrentUser = httpContextAccessor.HttpContext?.User;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (CurrentUser == null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        return Task.FromResult(new AuthenticationState(CurrentUser));
    }
}
