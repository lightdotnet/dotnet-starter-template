namespace StarterKit.Organization.Contracts.Employees;

public record EmployeeSearchRequest : SearchQuery
{
    public string? CompanyId { get; set; }

    public string? OrgUnitId { get; set; }

    public EmploymentStatus? EmploymentStatus { get; set; }
}
