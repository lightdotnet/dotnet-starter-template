namespace StarterKit.Identity.Contracts;

public class ServiceClaimDto
{
    public string Id { get; set; } = null!;

    public string OwnerService { get; set; } = null!;

    public string ClaimType { get; set; } = null!;

    public string ClaimValue { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string GroupName { get; set; } = null!;
}
