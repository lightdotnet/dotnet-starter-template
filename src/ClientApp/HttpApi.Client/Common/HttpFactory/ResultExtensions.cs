using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monolith.HttpApi.Common.HttpFactory;

public static class ResultExtensions
{
    // Single shared instance — thread-safe, avoids allocation on every call
    private static readonly JsonSerializerOptions _jsonOptions = BuildJsonOptions();

    private static JsonSerializerOptions BuildJsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    // ── JSON readers (no status validation — backend always returns Result) ──
    //
    // Design note on buffering:
    // ExecuteAsync uses HttpCompletionOption.ResponseHeadersRead, so response.Content
    // is a network stream that can only be read once. We buffer into a MemoryStream
    // first, then deserialize from that buffer. If deserialization fails, we rewind
    // and read the raw bytes for error diagnostics.
    //
    // This is more memory-efficient than ReadAsStringAsync (no UTF-16 expansion)
    // while still providing full raw body on error.

    public static async Task<Result> ReadResultAsync(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, ct).ConfigureAwait(false);
        buffer.Position = 0;

        try
        {
            var result = await JsonSerializer
                .DeserializeAsync<Result>(buffer, _jsonOptions, ct)
                .ConfigureAwait(false);

            return result ?? Result.Error("Result is null after deserialize.");
        }
        catch (Exception ex)
        {
            var raw = Encoding.UTF8.GetString(buffer.ToArray());
            return Result.Error($"Deserialize error: {ex.Message}. Raw: {raw}");
        }
    }

    public static async Task<Result<T>> ReadResultAsync<T>(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, ct).ConfigureAwait(false);
        buffer.Position = 0;

        try
        {
            var result = await JsonSerializer
                .DeserializeAsync<Result<T>>(buffer, _jsonOptions, ct)
                .ConfigureAwait(false);

            return result ?? Result<T>.Error(
                $"Result<{typeof(T).Name}> is null after deserialize.");
        }
        catch (Exception ex)
        {
            var raw = Encoding.UTF8.GetString(buffer.ToArray());
            return Result<T>.Error($"Deserialize error: {ex.Message}. Raw: {raw}");
        }
    }

    public static async Task<PagedResult<T>> ReadPagedResultAsync<T>(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        using var buffer = new MemoryStream();
        await response.Content.CopyToAsync(buffer, ct).ConfigureAwait(false);
        buffer.Position = 0;

        try
        {
            var result = await JsonSerializer
                .DeserializeAsync<PagedResult<T>>(buffer, _jsonOptions, ct)
                .ConfigureAwait(false);

            return result ?? PagedResultError<T>(
                $"PagedResult<{typeof(T).Name}> is null after deserialize.");
        }
        catch (Exception ex)
        {
            var raw = Encoding.UTF8.GetString(buffer.ToArray());
            return PagedResultError<T>($"Deserialize error: {ex.Message}. Raw: {raw}");
        }
    }

    public static PagedResult<T> PagedResultError<T>(string message) => new()
    {
        Code = ResultCode.Error,
        Message = message
    };

    // ── File reader (checks status because return type is Stream, not Result) ──

    /// <summary>
    /// Reads the response body into a self-contained MemoryStream.
    /// Copying to MemoryStream ensures the stream remains valid after the response is disposed.
    ///
    /// Unlike JSON readers, this method checks IsSuccessStatusCode because
    /// the return type is Stream — there is no Result wrapper to carry error info.
    /// Uses HttpRequestException (built-in .NET) to preserve StatusCode for logging/monitoring.
    /// </summary>
    public static async Task<Stream> ReadFileAsync(
        this HttpResponseMessage response,
        CancellationToken ct = default)
    {
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content
                .ReadAsStringAsync(ct)
                .ConfigureAwait(false);

            string errorMessage;
            try
            {
                var result = JsonSerializer.Deserialize<Result>(raw, _jsonOptions);
                errorMessage = result?.Message ?? raw;
            }
            catch (JsonException)
            {
                errorMessage = raw;
            }

            throw new HttpRequestException(
                $"File download failed ({(int)response.StatusCode} {response.ReasonPhrase}): {errorMessage}",
                inner: null,
                statusCode: response.StatusCode);
        }

        var memory = new MemoryStream();
        await response.Content.CopyToAsync(memory, ct).ConfigureAwait(false);
        memory.Position = 0;
        return memory;
    }
}
