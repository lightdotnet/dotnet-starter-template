using System.Collections.Concurrent;

namespace Monolith.Blazor.Services;

public class TokenMemoryStorage
{
    private readonly ConcurrentDictionary<string, TokenModel> _tokens = new();

    public Task<TokenModel?> GetAsync(string id)
    {
        _tokens.TryGetValue(id, out var token);
        return Task.FromResult(token);
    }

    public Task SaveAsync(string id, TokenModel data)
    {
        _tokens[id] = data;
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string id)
    {
        _tokens.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
