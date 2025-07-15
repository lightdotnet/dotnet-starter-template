namespace Monolith.Domain.Products;

public class ProductPrice : EntityBase
{
    public ProductPriceType Type { get; set; }

    public string ProductId { get; set; } = null!;

    public decimal Price { get; set; }

    public DateTime StartingDate { get; set; }

    public DateTime? EndingDate { get; set; }
}
