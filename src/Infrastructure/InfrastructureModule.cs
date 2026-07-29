using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using StarterKit.Mappings;
using StarterKit.Services;

namespace StarterKit;

public static class InfrastructureModule
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTime, DateTimeService>();

        MapsterSettings.Configure();

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder, bool allowAnonymous = false)
    {
        if (allowAnonymous)
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
