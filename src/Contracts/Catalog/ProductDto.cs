namespace Monolith.Catalog;

public class ProductDto : BaseDto
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string CategoryId { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string ShopId { get; set; } = null!;

    public string? MainImage { get; set; }

    public string Tags { get; set; } = null!;

    public string[] Images { get; set; } = [];
}
