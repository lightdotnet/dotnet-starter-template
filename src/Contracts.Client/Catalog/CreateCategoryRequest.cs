namespace Monolith.Catalog;

public record CreateCategoryRequest
{
    public string Name { get; set; } = null!;

    public string? SubOfId { get; set; }
}
