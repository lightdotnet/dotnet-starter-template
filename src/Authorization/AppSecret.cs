namespace Monolith;

/// <summary>
/// Default user with full permissions.
/// </summary>
public sealed class AppSecret
{
    public const string SUPER_USER_NAME = "super";

    private static List<string> SuperUsers =>
    [
        SUPER_USER_NAME,
        "minhvd"
    ];

    public static bool IsSuper(string? userName) => SuperUsers.Any(x => x == userName);
}