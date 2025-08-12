namespace Monolith.Catalog.Domain.Categories;

internal class CategoryByIdSpec : Specification<Category>
{
    public CategoryByIdSpec(string id)
    {
        Where(x => x.Id == id);
    }
}
