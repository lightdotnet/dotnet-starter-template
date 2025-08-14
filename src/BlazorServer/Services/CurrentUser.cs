using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Monolith.BlazorServer.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;
}


public class CurrentUser1(AuthenticationStateProvider state)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => ((JwtAuthStateProvider)state).CurrentUser;
}