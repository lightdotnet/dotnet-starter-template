using Light.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monolith.Identity.Jwt;

public static class DependencyInjection
{
    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var sectionName = "Jwt";

        // Override by BindConfiguration
        services.AddOptions<JwtOptions>().BindConfiguration(sectionName);
        services.AddJwtTokenProvider();

        services.AddScoped<ITokenService, TokenService>();

        // add JWT Auth
        var jwtSettings = configuration.GetSection(sectionName).Get<JwtOptions>();
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(JwtOptions));
        services.AddJwtAuth(jwtSettings.Issuer, jwtSettings.SecretKey, ClaimTypes.Role); // inject this for use jwt auth
    }
}