using Monolith.Authorization;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : CurrentUserBase, IClientCurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}