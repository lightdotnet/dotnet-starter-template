namespace Monolith;

/// <summary>
/// Default user with full permissions.
/// </summary>
public abstract class DefaultUser
{
    public const string USER_NAME = "super";

    public static List<string> MASTER_USERS =>
    [
        USER_NAME,
        "minhvd"
    ];
}

public abstract class DefaultRole
{
    public const string NAME = "super";

    public static List<string> MASTER_ROLES =>
    [
        NAME,
        "minhvd"
    ];
}
