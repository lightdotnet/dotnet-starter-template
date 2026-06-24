using System.Net.Http.Json;

namespace Monolith.HttpApi.Common.HttpFactory;

/// <summary>
/// Factory methods that build <see cref="HttpCallSpec"/> instances
/// for common HTTP verbs.
/// Content is always created lazily (deferred) so specs are safely reusable.
/// </summary>
public static class HttpCall
{
    // ── GET ───────────────────────────────────────────────────────

    public static HttpCallSpec Get(string url) => new()
    {
        Method = HttpMethod.Get,
        Url = url
    };

    // ── POST ──────────────────────────────────────────────────────

    public static HttpCallSpec Post<TBody>(string url, TBody body) => new()
    {
        Method = HttpMethod.Post,
        Url = url,
        ContentFactory = () => JsonContent.Create(body)
    };

    public static HttpCallSpec PostForm(
        string url,
        IEnumerable<KeyValuePair<string, string>> formData) => new()
    {
        Method = HttpMethod.Post,
        Url = url,
        ContentFactory = () => new FormUrlEncodedContent(formData)
    };

    // ── PUT ───────────────────────────────────────────────────────

    public static HttpCallSpec Put<TBody>(string url, TBody body) => new()
    {
        Method = HttpMethod.Put,
        Url = url,
        ContentFactory = () => JsonContent.Create(body)
    };

    // ── PATCH ─────────────────────────────────────────────────────

    public static HttpCallSpec Patch<TBody>(string url, TBody body) => new()
    {
        Method = HttpMethod.Patch,
        Url = url,
        ContentFactory = () => JsonContent.Create(body)
    };

    // ── DELETE ────────────────────────────────────────────────────

    public static HttpCallSpec Delete(string url) => new()
    {
        Method = HttpMethod.Delete,
        Url = url
    };

    public static HttpCallSpec Delete<TBody>(string url, TBody body) => new()
    {
        Method = HttpMethod.Delete,
        Url = url,
        ContentFactory = () => JsonContent.Create(body)
    };
}
