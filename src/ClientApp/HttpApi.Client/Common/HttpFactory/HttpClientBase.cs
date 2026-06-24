namespace Monolith.HttpApi.Common.HttpFactory;

public abstract class HttpClientBase(IHttpClientFactory httpClientFactory)
{
    /// <summary>
    /// Subclasses can override to specify a different named HttpClient.
    /// </summary>
    protected virtual string ClientName { get; } = HttpClientConstants.BackendApi;

    /// <summary>
    /// Default per-request timeout (seconds).
    /// Applied via CancellationToken inside HttpRequest.ExecuteAsync, not HttpClient.Timeout.
    /// Subclasses can override to change the default for all requests from this client.
    /// </summary>
    protected virtual int ClientTimeoutSeconds { get; } = 1800;

    /// <summary>
    /// Creates a named HttpClient instance from the factory.
    ///
    /// Safety guarantee for Timeout.InfiniteTimeSpan:
    /// ─────────────────────────────────────────────────
    /// HttpClient.Timeout is intentionally set to InfiniteTimeSpan because
    /// per-request timeout is controlled at execution level via linked
    /// CancellationTokenSource + CancelAfter inside HttpRequest.ExecuteAsync.
    ///
    /// This avoids "double timeout" behavior where HttpClient.Timeout races
    /// against the request-level timeout, producing unpredictable results.
    ///
    /// Every request that goes through Request&lt;T&gt;().ExecuteAsync() will always
    /// have a timeout applied (either explicit via WithTimeout or the default
    /// from ClientTimeoutSeconds), so no request can run infinitely unless
    /// the caller explicitly passes Timeout.InfiniteTimeSpan.
    /// </summary>
    internal HttpClient CreateClient()
    {
        var client = httpClientFactory.CreateClient(ClientName);
        client.Timeout = Timeout.InfiniteTimeSpan;
        return client;
    }

    /// <summary>
    /// Entry point for building a typed HTTP request.
    /// The compiler enforces the correct step order via interfaces:
    /// From → Read → [OnError] → [WithHeaders / WithTimeout] → ExecuteAsync
    /// </summary>
    public IHttpRequestFrom<TResult> Request<TResult>()
        => new HttpRequest<TResult>(CreateClient, TimeSpan.FromSeconds(ClientTimeoutSeconds));

    /// <summary>
    /// Downloads a file using GET (default) or POST when <paramref name="requestBody"/> is provided.
    /// Returns a self-contained MemoryStream that remains valid after the response is disposed.
    ///
    /// Goes through the standard pipeline (HttpRequest) so timeout and cancellation
    /// are handled consistently with all other requests.
    /// </summary>
    /// <param name="timeout">
    /// Per-request timeout override. Null falls back to ClientTimeoutSeconds.
    /// For large file downloads, pass a longer timeout, e.g. TimeSpan.FromMinutes(10).
    /// </param>
    /// <param name="ct">Token to cancel the download.</param>
    protected virtual Task<Stream> DownloadFileAsync(
        string url,
        object? requestBody = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        var request = Request<Stream>()
            .From(requestBody == null
                ? HttpCall.Get(url)
                : HttpCall.Post(url, requestBody))
            .Read((response, token) => response.ReadFileAsync(token));

        if (timeout.HasValue)
        {
            request = request.WithTimeout(timeout.Value);
        }

        // OnError is intentionally NOT called here:
        // DownloadFileAsync returns Stream, not Result.
        // Errors (network, timeout, non-success status) propagate as exceptions,
        // which is the correct contract for a method that returns Stream.

        return request.ExecuteAsync(ct);
    }

    /// <summary>
    /// Downloads a file and returns its contents as a Base64-encoded string.
    /// Optimized to avoid unnecessary memory copies when the underlying stream
    /// is already a MemoryStream (which ReadFileAsync always returns).
    /// </summary>
    protected virtual async Task<string> DownloadAsBase64Async(
        string url,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        await using var file = await DownloadFileAsync(url, timeout: timeout, ct: ct)
            .ConfigureAwait(false);

        // ReadFileAsync always returns a MemoryStream, so this path is the hot path.
        // TryGetBuffer avoids the extra copy that ToArray() would make.
        if (file is MemoryStream ms)
        {
            if (ms.TryGetBuffer(out var segment))
                return Convert.ToBase64String(segment.Array!, segment.Offset, segment.Count);

            return Convert.ToBase64String(ms.ToArray());
        }

        // Fallback for any future reader that returns a different stream type.
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct).ConfigureAwait(false);
        return Convert.ToBase64String(buffer.ToArray());
    }
}
