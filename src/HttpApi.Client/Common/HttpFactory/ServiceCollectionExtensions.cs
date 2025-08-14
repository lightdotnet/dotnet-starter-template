using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Monolith.HttpApi.Common.HttpFactory;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Auto scan and add httpclients
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<JwtAuthenticationHeaderHandler>();

        // get backend urls from section
        var backendUrls = configuration.GetSection("ApiUrls").Get<Dictionary<string, string>>();

        //ArgumentNullException.ThrowIfNull(backendUrls);
        if (backendUrls is null)
            return services;

        // auto add HttpClient with name & uri
        foreach (var backendUrl in backendUrls)
        {
            var clientName = backendUrl.Key;
            var uri = backendUrl.Value;

            if (string.IsNullOrEmpty(clientName) || string.IsNullOrEmpty(uri))
                continue;

            var httpClientBuilder = services
                .AddHttpClient(clientName, client =>
                {
                    client.BaseAddress = new Uri(uri);
                })
                .AddHttpMessageHandler<JwtAuthenticationHeaderHandler>();
        }

        return services;
    }

    /// <summary>
    /// Auto scan and add httpclient services
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        var typeOfBase = typeof(HttpClientBase);

        var implements = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(x => x.IsSubclassOf(typeOfBase) && x.IsClass && !x.IsAbstract)
            .Select(s => new
            {
                Interface = s.GetInterfaces().FirstOrDefault(),
                Service = s
            });

        foreach (var implement in implements)
        {
            if (implement.Interface is not null)
                services.AddScoped(implement.Interface, implement.Service);
            else
                services.AddScoped(implement.Service);
        }

        return services;
    }
}
