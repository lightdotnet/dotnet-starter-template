namespace Monolith.Authorization;

public static class AccessControl
{
    public static bool IsFullControl(this ICurrentUser currentUser) =>
        AppSecret.IsSuper(currentUser.Username);
}
