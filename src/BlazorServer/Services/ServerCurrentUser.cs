using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Monolith.BlazorServer.Services;

public class ServerCurrentUser1(AuthenticationStateProvider state)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => ((JwtAuthStateProvider)state).CurrentUser;
}

public class ServerCurrentUser(IHttpContextAccessor httpContextAccessor)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;
}