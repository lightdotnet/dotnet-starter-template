namespace Monolith.Catalog.Domain.Categories;

internal class CategoryByIdSpec : Specification<Category>
{
    public CategoryByIdSpec(string id)
    {
        Where(x => x.Id == id);
    }

    // Only main categories that are not disabled
    public CategoryByIdSpec IsMainCategory()
    {
        Where(x => string.IsNullOrEmpty(x.SubOfId));
        return this;
    }

    // Only main categories that are not disabled
    public CategoryByIdSpec MustActive()
    {
        Where(x => !x.Disable);
        return this;
    }
}
