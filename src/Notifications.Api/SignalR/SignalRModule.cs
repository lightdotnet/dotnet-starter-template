using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Infrastructure.Modularity;

namespace StarterKit.Notifications.Api.SignalR;

public class SignalRModule : AppModule
{
    public override void Add(IServiceCollection services)
    {
        services.AddSignalR();

        /* use only for Services API */
        services.AddSingleton<IUserIdProvider, CustomIdProvider>();

        services.AddScoped<SignalRHub>();

        services.AddScoped<IHubService, HubService>();

        ShowModuleInfo();
    }
}

public class SignalREndpoint : AppModuleEndpoint
{
    public override void Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<SignalRHub>("/signalr-hub", options =>
        {
            options.CloseOnAuthenticationExpiration = true;
            options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
        });
    }
}