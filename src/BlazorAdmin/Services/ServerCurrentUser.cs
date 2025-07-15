using Monolith;
using System.Security.Claims;

namespace BlazorAdmin.Services;

public class ServerCurrentUser(IHttpContextAccessor httpContextAccessor) : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}