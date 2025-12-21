namespace Monolith.Blazor.Shared;

public class AppClaimsVm
{
    public List<ClaimGroupDto> Groups { get; set; } = [];

    public List<AppClaimDto> Claims { get; set; } = [];
}

public record ClaimGroupDto(string Name, string? Description)
{
    public override string ToString()
    {
        var displayName = Name;
        if (!string.IsNullOrEmpty(Description))
        {
            displayName += $" ({Description})";
        }

        return displayName;
    }
}

public record AppClaimDto(string GroupName, string Value, string Name, string? Description)
{
    public bool IsOwned { get; set; }

    public override string ToString()
    {
        var displayName = Name;
        if (!string.IsNullOrEmpty(Description))
        {
            displayName += $" ({Description})";
        }

        return displayName;
    }
}
