namespace Monolith;

public interface ICurrentUser
{
    string? UserId { get; }

    string? Username { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);

    bool HasPermission(string permission);
}
