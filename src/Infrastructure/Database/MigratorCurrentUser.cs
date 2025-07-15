namespace Monolith.Database;

internal class MigratorCurrentUser : ICurrentUser
{
    public string? UserId => "Migrator";

    public string? Username => null;

    public string? FirstName => null;

    public string? LastName => null;

    public string? FullName => null;

    public string? PhoneNumber => null;

    public string? Email => null;

    public bool IsAuthenticated => false;

    public bool IsMasterUser => false;

    public bool HasPermission(string permission)
    {
        throw new NotImplementedException();
    }

    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }
}
