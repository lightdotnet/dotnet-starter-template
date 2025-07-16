using System.Text.Json;
using System.Text.Json.Serialization;

namespace Monolith;

public class HttpClientConsts
{
    public const string BackendApi = nameof(BackendApi);

    public static JsonSerializerOptions Default()
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
}
