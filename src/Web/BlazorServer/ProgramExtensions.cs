using Light.Extensions.DependencyInjection;
using Light.MudBlazor;
using Monolith.HttpApi;
using Monolith.HttpApi.Common.HttpFactory;
using Monolith.HttpApi.Common.Interfaces;
using Monolith.Infrastructure.Services;
using Monolith.Services;
using MudBlazor.Services;

namespace Monolith;

public static class ProgramExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMudServices();
        services.AddMudBlazorExtraComponents();

        services.AddHttpClients(configuration);
        services.AddHttpClientServices(typeof(HttpApiClientModule).Assembly);

        services.AddFileGenerator();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();

        //services.AddDefaultPermissionManager();
        services.AddPermissionManager<AppPermissionManager>();
        services.AddPermissionPolicy();
        services.AddPermissionAuthorization();

        services.AddTransient<ITokenProvider, TokenProvider>();

        return services;
    }
}