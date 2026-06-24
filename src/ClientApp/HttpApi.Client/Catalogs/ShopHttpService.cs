using Monolith.Catalog;

namespace Monolith.HttpApi.Catalogs;

public class ShopHttpService(IHttpClientFactory httpClientFactory)
    : HttpClientBase(httpClientFactory)
{
    public const string BasePath = "shop";

    public Task<PagedResult<ShopDto>> SearchAsync(ShopLookup lookup)
    {
        var url = $"{BasePath}/search";

        return this.TryPagedAsync<ShopDto>(url, lookup);
    }

    public Task<Result> CreateAsync(CreateShopRequest request)
    {
        var url = BasePath;

        return this.TryPostAsync(url, request);
    }

    public Task<Result> UpdateAsync(ShopDto request)
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
