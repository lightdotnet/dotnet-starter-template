using System.Security.Claims;

namespace Monolith.BlazorServer.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;
}