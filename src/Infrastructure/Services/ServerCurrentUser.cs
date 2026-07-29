using Microsoft.AspNetCore.Http;
using StarterKit.Authorization;
using System.Security.Claims;

namespace StarterKit.Services;

public class ServerCurrentUser(IHttpContextAccessor httpContextAccessor)
    : CurrentUserBase, ICurrentUser
{
    public override ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;
}