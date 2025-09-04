using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monolith.HttpApi.Common.HttpFactory;

public static class ResultExtensions
{
    private static JsonSerializerOptions JsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    public async static Task<Result> ReadResultAsync(this HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<Result>(responseString, JsonSerializerOptions());

        if (result == null)
        {
            return Result.Error($"Error when deserialize result: {responseString}");
        }

        return result;
    }

    public async static Task<Result<T>> ReadResultAsync<T>(this HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<Result<T>>(responseString, JsonSerializerOptions());

        if (result == null)
        {
            return Result<T>.Error($"Error when deserialize result of {nameof(T)}: {responseString}");
        }

        return result;
    }

    public async static Task<PagedResult<T>> ReadPagedResultAsync<T>(this HttpResponseMessage response)
    {
        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<PagedResult<T>>(responseString, JsonSerializerOptions());

        if (result == null)
        {
            return new PagedResult<T>
            {
                Code = ResultCode.error.ToString(),
                Message = $"Error when deserialize result of {nameof(T)}: {responseString}"
            };
        }

        return result;
    }

    public static async Task<Stream> ReadFileAsync(this HttpResponseMessage message)
    {
        if (message.IsSuccessStatusCode is false)
        {
            var responseString = await message.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<Result>(responseString, JsonSerializerOptions());

            ArgumentNullException.ThrowIfNull(result);

            throw new Exception(result.Message);
        }

        var stream = await message.Content.ReadAsStreamAsync();

        return stream;
    }
}