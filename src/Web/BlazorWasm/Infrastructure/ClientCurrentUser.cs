using Monolith.Core;
using System.Security.Claims;

namespace Monolith.Infrastructure;

public class ClientCurrentUser(IIdentityManager identityManager) : CurrentUserBase, ICurrentUser
{
    protected override ClaimsPrincipal? User => identityManager.CurrentUser;
}