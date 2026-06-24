namespace Monolith.HttpApi.Common.HttpFactory;

public static class HttpClientBaseExtensions
{
    // ── GET ───────────────────────────────────────────────────────

    public static Task<Result> TryGetAsync(
        this HttpClientBase client,
        string url,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Get(url))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryGetAsync<T>(
        this HttpClientBase client,
        string url,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Get(url))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .ExecuteAsync(ct);

    // ── POST ──────────────────────────────────────────────────────

    public static Task<Result> TryPostAsync(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Post(url, body))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryPostAsync<T>(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Post(url, body))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .ExecuteAsync(ct);

    // ── PUT ───────────────────────────────────────────────────────

    public static Task<Result> TryPutAsync(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Put(url, body))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryPutAsync<T>(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Put(url, body))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .ExecuteAsync(ct);

    // ── PATCH ─────────────────────────────────────────────────────

    public static Task<Result> TryPatchAsync(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Patch(url, body))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryPatchAsync<T>(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Patch(url, body))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .ExecuteAsync(ct);

    // ── DELETE ────────────────────────────────────────────────────

    public static Task<Result> TryDeleteAsync(
        this HttpClientBase client,
        string url,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Delete(url))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    public static Task<Result> TryDeleteAsync(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Delete(url, body))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .ExecuteAsync(ct);

    // ── PAGED ─────────────────────────────────────────────────────

    public static Task<PagedResult<T>> TryPagedAsync<T>(
        this HttpClientBase client,
        string url,
        CancellationToken ct = default)
        => client.Request<PagedResult<T>>()
            .From(HttpCall.Get(url))
            .Read((r, token) => r.ReadPagedResultAsync<T>(token))
            .OnError(ResultExtensions.PagedResultError<T>)
            .ExecuteAsync(ct);

    public static Task<PagedResult<T>> TryPagedAsync<T>(
        this HttpClientBase client,
        string url,
        object body,
        CancellationToken ct = default)
        => client.Request<PagedResult<T>>()
            .From(HttpCall.Post(url, body))
            .Read((r, token) => r.ReadPagedResultAsync<T>(token))
            .OnError(ResultExtensions.PagedResultError<T>)
            .ExecuteAsync(ct);

    // ── WITH HEADERS ──────────────────────────────────────────────

    public static Task<Result> TryGetAsync(
        this HttpClientBase client,
        string url,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Get(url))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .WithHeaders(headers)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryGetAsync<T>(
        this HttpClientBase client,
        string url,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Get(url))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .WithHeaders(headers)
            .ExecuteAsync(ct);

    public static Task<Result> TryPostAsync(
        this HttpClientBase client,
        string url,
        object body,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
        => client.Request<Result>()
            .From(HttpCall.Post(url, body))
            .Read((r, token) => r.ReadResultAsync(token))
            .OnError(Result.Error)
            .WithHeaders(headers)
            .ExecuteAsync(ct);

    public static Task<Result<T>> TryPostAsync<T>(
        this HttpClientBase client,
        string url,
        object body,
        IReadOnlyDictionary<string, string> headers,
        CancellationToken ct = default)
        => client.Request<Result<T>>()
            .From(HttpCall.Post(url, body))
            .Read((r, token) => r.ReadResultAsync<T>(token))
            .OnError(Result<T>.Error)
            .WithHeaders(headers)
            .ExecuteAsync(ct);
}
