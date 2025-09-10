using Monolith.Identity;
using Monolith.Identity.Jwt;
using Monolith.Modularity;

namespace Monolith.Features.Identity;

public class IdentityModule : AppModule
{
    public override void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityModule(configuration);

        services.AddJwtAuthentication(configuration);

        AppLogging.ModuleInjected(GetType().Name);
    }
}
