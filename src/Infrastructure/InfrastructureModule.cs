using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Mappings;
using Monolith.Services;

namespace Monolith;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, DateTimeService>();

        MapsterSettings.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder, bool allowAnonymous = false)
    {
        var isDebug = false;

#if DEBUG
        //isDebug = true;
#endif

        if (allowAnonymous || isDebug)
        {
            builder.MapControllers().AllowAnonymous();
        }
        else
        {
            builder.MapControllers().RequireAuthorization();
        }

        return builder;
    }
}