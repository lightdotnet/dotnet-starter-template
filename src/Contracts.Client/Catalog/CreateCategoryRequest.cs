namespace Monolith.Catalog;

public record CreateCategoryRequest(string Name, string? SubOfId = null);
