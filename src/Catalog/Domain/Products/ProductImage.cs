namespace Monolith.Domain.Products;

public class ProductImage : EntityBase
{
    public string ProductId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
