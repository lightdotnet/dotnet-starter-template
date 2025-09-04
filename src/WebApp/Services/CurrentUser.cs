using System.Security.Claims;

namespace Monolith.WebAdmin.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor)
    : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;
}