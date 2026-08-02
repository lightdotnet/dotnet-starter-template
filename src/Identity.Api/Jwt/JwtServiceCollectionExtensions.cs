using Light.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Shared.Constants;

namespace StarterKit.Identity.Api.Jwt;

public static class JwtServiceCollectionExtensions
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Override by BindConfiguration
        var sectionName = "Jwt";
        services.AddOptions<JwtOptions>().BindConfiguration(sectionName);
        var jwtSettings = configuration.GetSection(sectionName).Get<JwtOptions>();
        ArgumentNullException.ThrowIfNull(jwtSettings, nameof(jwtSettings));

        // inject this for use jwt auth
        services.AddJwtAuth(
            jwtSettings.Issuer,
            jwtSettings.SecretKey,
            ClaimTypeConstants.Role);

        // services
        services.AddSingleton<JwtSigningService>();
        services.AddScoped<JwtTokenIssuer>();
        services.AddScoped<IUserSessionService, UserSessionService>();
        services.AddTransient<IAuthenticationService, AuthenticationService>();
    }
}