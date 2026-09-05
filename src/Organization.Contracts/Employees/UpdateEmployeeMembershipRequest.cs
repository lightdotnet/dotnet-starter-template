namespace StarterKit.Organization.Contracts.Employees;

public record UpdateEmployeeMembershipRequest
{
    public string? LevelId { get; set; }

    public bool IsPrimary { get; set; }
}
