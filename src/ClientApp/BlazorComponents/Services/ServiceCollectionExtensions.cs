using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.HttpApi.Common.HttpFactory;

namespace Monolith.Blazor.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Auto scan and add httpclients
    ///     Please add backend urls in configuration section "ApiUrls"
    /// </summary>
    public static IServiceCollection AddHttpClientsWithJwtMessageHandler(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<JwtAuthenticationHeaderHandler>();

        services.AddHttpClients(configuration, builder =>
        {
            builder.AddHttpMessageHandler<JwtAuthenticationHeaderHandler>();
        });

        return services;
    }
}
