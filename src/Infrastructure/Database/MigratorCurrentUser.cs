namespace Monolith.Database;

internal class MigratorCurrentUser : ICurrentUser
{
    public string? UserId => "Migrator";

    public string? Username => throw new NotImplementedException();

    public string? FirstName => throw new NotImplementedException();

    public string? LastName => throw new NotImplementedException();

    public string? FullName => throw new NotImplementedException();

    public string? PhoneNumber => throw new NotImplementedException();

    public string? Email => throw new NotImplementedException();

    public bool IsAuthenticated => throw new NotImplementedException();

    public bool IsMasterUser => throw new NotImplementedException();

    public string? EmployeeId => throw new NotImplementedException();

    public bool HasPermission(string permission)
    {
        throw new NotImplementedException();
    }

    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }
}
