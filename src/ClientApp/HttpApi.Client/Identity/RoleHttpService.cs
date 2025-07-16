using Light.Identity;

namespace Monolith.Identity;

public class RoleHttpService(IHttpClientFactory httpClientFactory) :
    TryHttpClient(httpClientFactory)
{
    private const string _path = "api/v1/role";

    public Task<Result<IEnumerable<RoleDto>>> GetAsync()
    {
        var url = _path;

        return TryGetAsync<IEnumerable<RoleDto>>(url);
    }

    public Task<Result<RoleDto>> GetByIdAsync(string id)
    {
        var url = $"{_path}/{id}";

        return TryGetAsync<RoleDto>(url);
    }

    public Task<Result> CreateAsync(CreateRoleRequest request)
    {
        var url = _path;

        return TryPostAsync(url, request);
    }

    public Task<Result> UpdateAsync(RoleDto request)
    {
        var url = $"{_path}";

        return TryPutAsync(url, request);
    }

    public Task<Result> DeleteAsync(string id)
    {
        var url = $"{_path}/{id}";

        return TryDeleteAsync(url);
    }
}