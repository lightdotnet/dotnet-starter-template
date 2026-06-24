namespace Monolith.HttpApi.Common.HttpFactory;

/// <summary>
/// Step 1 — Required: define the HTTP call to execute.
/// Only .From() is available at this stage.
/// </summary>
public interface IHttpRequestFrom<TResult>
{
    IHttpRequestRead<TResult> From(HttpCallSpec spec);
}

/// <summary>
/// Step 2 — Required: define how to read the HttpResponseMessage into TResult.
/// </summary>
public interface IHttpRequestRead<TResult>
{
    IHttpRequestReady<TResult> Read(
        Func<HttpResponseMessage, CancellationToken, Task<TResult>> reader);
}

/// <summary>
/// Step 3 — Optional configuration + execution.
/// OnError is optional: when omitted, exceptions bubble up to the caller.
/// WithHeaders and WithTimeout can be called in any order, any number of times.
/// ExecuteAsync is the single termination point.
/// </summary>
public interface IHttpRequestReady<TResult>
{
    IHttpRequestReady<TResult> OnError(Func<string, TResult> onError);
    IHttpRequestReady<TResult> WithHeaders(IReadOnlyDictionary<string, string> headers);
    IHttpRequestReady<TResult> WithTimeout(TimeSpan timeout);
    Task<TResult> ExecuteAsync(CancellationToken ct = default);
}
