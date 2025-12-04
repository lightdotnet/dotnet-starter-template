using Blazored.LocalStorage;
using Light.Blazor;

namespace Monolith.Blazor.Services;

public class StorageService(ILocalStorageService localStorage) : IStorageService
{
    public ValueTask<T?> GetAsync<T>(string key) => localStorage.GetItemAsync<T>(key);

    public ValueTask SetAsync<T>(string key, T data) => localStorage.SetItemAsync(key, data);

    public ValueTask RemoveAsync(string key) => localStorage.RemoveItemAsync(key);

    public ValueTask ClearAsync() => localStorage.ClearAsync();
}
