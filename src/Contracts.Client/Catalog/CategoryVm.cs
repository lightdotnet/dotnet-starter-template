namespace Monolith.Catalog;

public class CategoryVm : CategoryDto
{
    public IEnumerable<CategoryDto> SubCategories { get; set; } = [];
}
