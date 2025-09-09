using System.Net.Http.Headers;

namespace Monolith.HttpApi.Common.HttpFactory;

public abstract class HttpClientBase(IHttpClientFactory httpClientFactory)
{
    // for inherit classes can overide ClientName
    protected virtual string ClientName { get; } = HttpClientConstants.BackendApi;

    // for inherit classes can config client timeout
    protected virtual int ClientTimeoutSeconds { get; } = 1800;

    private HttpClient? _httpClient;

    protected HttpClient HttpClient
    {
        get
        {
            _httpClient ??= CreateClient();

            return _httpClient;
        }
    }

    private HttpClient CreateClient(string? token = null)
    {
        var client = httpClientFactory.CreateClient(ClientName);
        client.Timeout = TimeSpan.FromSeconds(ClientTimeoutSeconds);
        client.DefaultRequestHeaders.Clear();

        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    protected async Task<Stream> DownloadFileAsync(string url)
    {
        return await (await HttpClient.GetAsync(url)).ReadFileAsync();
    }

    protected async Task<Stream> DownloadFileAsync(string url, object request)
    {
        return await (await HttpClient.PostAsJsonAsync(url, request)).ReadFileAsync();
    }

    protected async Task<string> DownloadAsBase64Async(string url)
    {
        var file = await DownloadFileAsync(url);

        var base64Content = file.ToBase64String();

        return base64Content;
    }
}