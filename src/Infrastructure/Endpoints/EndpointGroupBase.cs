using Microsoft.AspNetCore.Builder;

namespace Monolith.Endpoints;

public abstract class EndpointGroupBase
{
    public abstract void Map(WebApplication app);
}
