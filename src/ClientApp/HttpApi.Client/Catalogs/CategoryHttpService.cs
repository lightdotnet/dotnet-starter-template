using Monolith.Catalog;

namespace Monolith.HttpApi.Catalogs;

public class CategoryHttpService(IHttpClientFactory httpClientFactory)
    : HttpClientBase(httpClientFactory)
{
    public const string BasePath = "category";

    public Task<Result<IEnumerable<CategoryVm>>> GetAsync()
    {
        var url = BasePath;

        return this.TryGetAsync<IEnumerable<CategoryVm>>(url);
    }

    public Task<Result> CreateAsync(CreateCategoryRequest request)
    {
        var url = BasePath;

        return this.TryPostAsync(url, request);
    }

    public Task<Result> UpdateAsync(CategoryDto request)
    {
        var url = BasePath;

        return this.TryPutAsync(url, request);
    }

    public Task<Result> DeleteAsync(string id)
    {
        var url = $"{BasePath}/{id}";

        return this.TryDeleteAsync(url);
    }
}
