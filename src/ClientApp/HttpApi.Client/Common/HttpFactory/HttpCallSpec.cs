namespace Monolith.HttpApi.Common.HttpFactory;

/// <summary>
/// Describes an HTTP request (method, URL, optional content).
/// Content is created lazily via <see cref="ContentFactory"/> so the spec
/// can be safely reused across multiple executions.
/// </summary>
public sealed class HttpCallSpec
{
    public required HttpMethod Method { get; init; }
    public required string Url { get; init; }

    /// <summary>
    /// Factory that produces the request body on each execution.
    /// Deferred creation ensures HttpContent is never reused after disposal.
    /// Null means no body (e.g. GET, DELETE without body).
    /// </summary>
    public Func<HttpContent>? ContentFactory { get; init; }

    /// <summary>
    /// Creates a fresh <see cref="HttpRequestMessage"/> for one execution cycle.
    /// Caller is responsible for disposing the returned message.
    /// </summary>
    internal HttpRequestMessage ToMessage() => new(Method, Url)
    {
        Content = ContentFactory?.Invoke()
    };
}
