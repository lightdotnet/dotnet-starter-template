using Microsoft.AspNetCore.Components.Authorization;
using Monolith.Authorization;
using System.Security.Claims;

namespace Monolith.Blazor.Services;

public class ClientCurrentUser(AuthenticationStateProvider authenticationStateProvider)
    : CurrentUserBase, IClientCurrentUser
{
    protected override ClaimsPrincipal? User =>
        ((JwtAuthenticationStateProvider)authenticationStateProvider).CurrentUser;
}