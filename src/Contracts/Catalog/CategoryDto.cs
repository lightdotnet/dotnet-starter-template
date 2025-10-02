namespace Monolith.Catalog;

public class CategoryDto : BaseDto
{
    public string Name { get; set; } = null!;

    public bool Disable { get; set; }
}
