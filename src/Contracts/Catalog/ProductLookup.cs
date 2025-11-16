namespace Monolith.Catalog;

public record ProductLookup : Pagination
{
    public string? Search { get; set; }

    public string? ShopId { get; set; }
}
