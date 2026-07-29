using System.ComponentModel.DataAnnotations;

namespace StarterKit.Identity;

public class RegisterServiceClaimsRequest
{
    [Required]
    public string OwnerService { get; set; } = null!;

    public IList<ServiceClaimEntryDto> Claims { get; set; } = [];
}

public class ServiceClaimEntryDto
{
    [Required]
    public string ClaimType { get; set; } = null!;

    [Required]
    public string ClaimValue { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public string GroupName { get; set; } = null!;
}
