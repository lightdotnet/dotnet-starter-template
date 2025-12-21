using Microsoft.AspNetCore.Http;
using Monolith.Authorization;
using System.Security.Claims;

namespace Monolith.Services;

public class ServerCurrentUser(IHttpContextAccessor httpContextAccessor) : CurrentUserBase, ICurrentUser
{
    public override ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}