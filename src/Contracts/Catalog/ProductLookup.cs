namespace Monolith.Catalog;

public record ProductLookup : PageQuery
{
    public string? Search { get; set; }

    public string? ShopId { get; set; }
}
