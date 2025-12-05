using Light.Extensions.DependencyInjection;
using Light.MudBlazor;
using Monolith.Blazor.Services;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;

namespace Monolith.Blazor;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFileGenerator();
        services.AddMudBlazorExtraComponents();

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddCascadingAuthenticationState();

        services.AddScoped<LayoutService>();

        // WebServer
        services.AddSingleton<TokenMemoryStorage>();

        services.AddScoped<ITokenProvider, TokenProvider>();

        return services;
    }
}
