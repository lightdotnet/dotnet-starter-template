using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Authorization;
using Monolith.Blazor.Services;
using System.Security.Claims;

namespace Monolith.Blazor.Infrastructure;

public class ClientCurrentUser(AuthenticationStateProvider authenticationStateProvider)
    : CurrentUserBase, IClientCurrentUser
{
    public override ClaimsPrincipal? User =>
        ((JwtAuthenticationStateProvider)authenticationStateProvider).CurrentUser;
}