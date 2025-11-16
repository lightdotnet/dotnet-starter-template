using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Infrastructure.Services;

internal class TokenProvider : ITokenProvider
{
    public Task<string?> GetAccessTokenAsync()
    {
        return Task.FromResult<string?>("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwMUtBNTlOTUYxMVIyV1lCV1NSU1FNR1E2ViIsInVuIjoic3VwZXIiLCJ0aWQiOiIwMUtBNVhLNTJZWDIzTjJKOVgwMFlaNTIxTiIsImV4cCI6MTc2MzI4NjQ4MywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3QifQ.MZFX87DZeqyNUHyAEWVeGvmrCbDQk_ZNainVFbfe3C8");
    }
}
