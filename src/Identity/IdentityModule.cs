using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Identity.Jwt;
using StarterKit.Modularity;

namespace StarterKit.Identity;

public class IdentityModule : AppModule
{
    public override void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityServices(configuration);

        services.AddJwtAuthentication(configuration);

        ShowModuleInfo();
    }
}
