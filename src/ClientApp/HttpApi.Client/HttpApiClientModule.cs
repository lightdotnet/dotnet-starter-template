using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Monolith;

public class HttpApiClientModule;

public static class Startup
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<JwtAuthenticationHeaderHandler>();

        // get backend urls from section
        var backendUrls = configuration.GetSection("BackendUrls").Get<Dictionary<string, string>>();

        ArgumentNullException.ThrowIfNull(backendUrls);

        // auto add HttpClient with name & uri
        foreach (var backendUrl in backendUrls)
        {
            var clientName = backendUrl.Key;
            var uri = backendUrl.Value;

            if (string.IsNullOrEmpty(clientName) || string.IsNullOrEmpty(uri))
                continue;

            services
                .AddHttpClient(clientName, client =>
                {
                    client.BaseAddress = new Uri(uri);
                })
                .AddHttpMessageHandler<JwtAuthenticationHeaderHandler>()
                .Services
                .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName));
        }
        /*
        var apiClients = typeof(HttpClientConsts).GetStaticValues();
        foreach (var client in apiClients)
        {
            var clientName = client.Value.ToString();

            if (string.IsNullOrEmpty(clientName)) continue;

            var uri = configuration[clientName];

            if (string.IsNullOrEmpty(uri)) continue;

            services
                .AddHttpClient(clientName, client =>
                {
                    client.BaseAddress = new Uri(uri);
                })
                .AddHttpMessageHandler<JwtAuthenticationHeaderHandler>()
                .Services
                .AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient(clientName));
        }
        */

        return services;
    }

    public static IServiceCollection AddHttpServices(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var typeOfBase = typeof(HttpClientBase);

        var implements = assemblies
            .SelectMany(s => s.GetExportedTypes())
            .Where(x => x.IsSubclassOf(typeOfBase) && x.IsClass && !x.IsAbstract);

        foreach (var implement in implements)
        {
            services.AddScoped(implement);
        }

        return services;
    }
}