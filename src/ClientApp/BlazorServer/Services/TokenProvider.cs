using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Blazor.Services;

public class TokenProvider : ITokenProvider
{
    public Task<string?> GetAccessTokenAsync()
    {
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiIwMUtBNTlOTUYxMVIyV1lCV1NSU1FNR1E2ViIsInVuIjoic3VwZXIiLCJ0aWQiOiIwMUtCUTZIQTE4OFhSVFdRNzVYVEhKUkQwVCIsImV4cCI6MTc2NDk0MDAyNywiaXNzIjoiaHR0cHM6Ly9sb2NhbGhvc3QifQ.lX35OsGuTU-tNrbjuI9V3seOE6l9TxR7Q-3EzeYJC7s";

        return Task.FromResult<string?>(token);
    }
}
