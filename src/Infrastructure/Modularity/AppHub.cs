using Microsoft.AspNetCore.Routing;

namespace Monolith.Modularity;

public abstract class AppHub : Light.AspNetCore.Modularity.IModuleEndpoint
{
    public virtual void Map(IEndpointRouteBuilder endpoints)
    { }
}
