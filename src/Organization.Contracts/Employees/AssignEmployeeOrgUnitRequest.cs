namespace StarterKit.Organization.Contracts.Employees;

public record AssignEmployeeOrgUnitRequest
{
    public string OrgUnitId { get; set; } = null!;

    public string? LevelId { get; set; }

    public bool IsPrimary { get; set; }
}
