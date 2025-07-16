using Light.Extensions.DependencyInjection;
using Light.Identity;
using Light.Identity.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Database;
using Monolith.Identity.Data;
using Monolith.Modularity;

namespace Monolith.Identity;

public class IdentityModule : AppModule
{
    public override void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(configuration, DbConnectionNames.IDENTITY);

        services.AddIdentity<AppIdentityDbContext>(options =>
        {
            options.SignIn.RequireConfirmedEmail = false;

            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // Lockout settings
            //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
            //options.Lockout.MaxFailedAccessAttempts = 10;

            // User settings
            options.User.RequireUniqueEmail = false;
        });

        AddAuth(services, configuration);
    }

    private void AddAuth(IServiceCollection services, IConfiguration configuration)
    {
        var sectionName = "Jwt";

        // Override by BindConfiguration
        services.AddOptions<JwtOptions>().BindConfiguration(sectionName);
        services.AddJwtTokenProvider();

        // add JWT Auth
        var jwtSettings = configuration.GetSection(sectionName).Get<JwtOptions>();
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(JwtOptions));
        services.AddJwtAuth(jwtSettings.Issuer, jwtSettings.SecretKey, ClaimTypes.Role); // inject this for use jwt auth
    }
}