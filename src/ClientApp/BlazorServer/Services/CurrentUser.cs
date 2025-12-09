using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Authorization;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : CurrentUserBase, IClientCurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}

public class CurrentUser1(AuthenticationStateProvider authenticationStateProvider) : CurrentUserBase, IClientCurrentUser
{
    protected override ClaimsPrincipal? User =>
        (authenticationStateProvider as JwtAuthenticationStateProviderServer)?.CurrentUser;
}

