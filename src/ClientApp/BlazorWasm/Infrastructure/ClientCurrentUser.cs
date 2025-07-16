using BlazorApp.Core;
using System.Security.Claims;

namespace BlazorApp.Infrastructure;

public class ClientCurrentUser(IIdentityManager identityManager) : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => identityManager.CurrentUser;
}