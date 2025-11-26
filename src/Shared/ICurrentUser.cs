namespace Monolith;

public interface ICurrentUser
{
    string? UserId { get; }

    string? Username { get; }

    string? FirstName { get; }

    string? LastName { get; }

    string? FullName { get; }

    string? PhoneNumber { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);

    bool HasPermission(string permission);
}
