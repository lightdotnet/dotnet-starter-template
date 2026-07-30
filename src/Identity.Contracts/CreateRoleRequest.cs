namespace StarterKit.Identity.Contracts;

public class CreateRoleRequest
{
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
