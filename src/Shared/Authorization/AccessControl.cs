using StarterKit.Extensions;
using System.Security.Claims;

namespace StarterKit.Authorization;

public static class AccessControl
{
    public static bool IsFullControl(this ICurrentUser currentUser) =>
        SuperUserPolicy.IsSuper(currentUser.Username);

    public static bool IsFullControl(this ClaimsPrincipal? claimsPrincipal) =>
        SuperUserPolicy.IsSuper(claimsPrincipal?.GetUserName());
}
