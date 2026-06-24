namespace Monolith.HttpApi.Common.HttpFactory;

/// <summary>
/// Internal implementation that satisfies all builder interfaces.
/// End-users only see the interface corresponding to their current step,
/// preventing out-of-order calls or skipping required steps.
/// </summary>
internal sealed class HttpRequest<TResult> :
    IHttpRequestFrom<TResult>,
    IHttpRequestRead<TResult>,
    IHttpRequestReady<TResult>
{
    private readonly Func<HttpClient> _clientFactory;
    private readonly TimeSpan _defaultTimeout;

    private HttpCallSpec? _spec;
    private Func<HttpResponseMessage, CancellationToken, Task<TResult>>? _reader;
    private Func<string, TResult>? _onError;
    private Dictionary<string, string>? _headers;
    private TimeSpan? _timeout;

    internal HttpRequest(Func<HttpClient> clientFactory, TimeSpan defaultTimeout)
    {
        if (defaultTimeout < TimeSpan.Zero && defaultTimeout != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(defaultTimeout),
                "Default timeout must be positive, zero, or Timeout.InfiniteTimeSpan.");

        _clientFactory = clientFactory;
        _defaultTimeout = defaultTimeout;
    }

    // ── Step 1 ────────────────────────────────────────────────────

    IHttpRequestRead<TResult> IHttpRequestFrom<TResult>.From(HttpCallSpec spec)
    {
        _spec = spec;
        return this;
    }

    // ── Step 2 ────────────────────────────────────────────────────

    IHttpRequestReady<TResult> IHttpRequestRead<TResult>.Read(
        Func<HttpResponseMessage, CancellationToken, Task<TResult>> reader)
    {
        _reader = reader;
        return this;
    }

    // ── Step 3 (all optional + execute) ───────────────────────────

    IHttpRequestReady<TResult> IHttpRequestReady<TResult>.OnError(
        Func<string, TResult> onError)
    {
        _onError = onError;
        return this;
    }

    IHttpRequestReady<TResult> IHttpRequestReady<TResult>.WithHeaders(
        IReadOnlyDictionary<string, string> headers)
    {
        _headers ??= new Dictionary<string, string>();

        foreach (var (key, value) in headers)
        {
            _headers[key] = value; // merge, last-win
        }

        return this;
    }

    IHttpRequestReady<TResult> IHttpRequestReady<TResult>.WithTimeout(TimeSpan timeout)
    {
        if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
            throw new ArgumentOutOfRangeException(nameof(timeout),
                "Timeout must be positive, zero, or Timeout.InfiniteTimeSpan.");

        _timeout = timeout;
        return this;
    }

    // ── Execute ───────────────────────────────────────────────────

    async Task<TResult> IHttpRequestReady<TResult>.ExecuteAsync(CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(_spec);
        ArgumentNullException.ThrowIfNull(_reader);

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            var effectiveTimeout = _timeout ?? _defaultTimeout;
            ApplyTimeoutIfNeeded(linkedCts, effectiveTimeout);

            var client = _clientFactory();

            // HttpCallSpec.ToMessage() creates content lazily via ContentFactory.
            // Headers from WithHeaders() are merged on top.
            using var message = _spec.ToMessage();

            if (_headers != null)
            {
                foreach (var (key, value) in _headers)
                {
                    message.Headers.TryAddWithoutValidation(key, value);
                }
            }

            // ResponseHeadersRead: Task completes as soon as headers arrive,
            // body is streamed on demand — reduces peak memory ~50%.
            using var response = await client
                .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token)
                .ConfigureAwait(false);

            return await _reader(response, linkedCts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_onError is not null)
        {
            return _onError("[OperationCanceledException] Request was cancelled.");
        }
        catch (Exception ex) when (_onError is not null)
        {
            return _onError($"[{ex.GetType().Name}] {ex.Message}");
        }
        // When _onError is null, exceptions bubble up naturally.
    }

    /// <summary>
    /// Applies the effective timeout to the linked CancellationTokenSource.
    /// Handles all edge cases:
    /// - InfiniteTimeSpan (-1ms): no internal timeout, external CT still works.
    /// - Zero: immediate cancellation (useful for testing / circuit-breaker).
    /// - Positive: standard CancelAfter.
    /// </summary>
    private static void ApplyTimeoutIfNeeded(CancellationTokenSource cts, TimeSpan timeout)
    {
        if (timeout == Timeout.InfiniteTimeSpan)
            return;

        if (timeout == TimeSpan.Zero)
        {
            cts.Cancel();
            return;
        }

        if (timeout > TimeSpan.Zero)
        {
            cts.CancelAfter(timeout);
        }
    }
}
