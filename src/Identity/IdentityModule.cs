using Light.ActiveDirectory;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monolith.Database;
using Monolith.Identity.Data;
using Monolith.Identity.Jwt;
using Monolith.Modularity;

namespace Monolith.Identity;

public class IdentityModule : AppModule
{
    public override void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppIdentityDbContext>(configuration, DbConnectionNames.IDENTITY);
        
        services.AddJwt(configuration);

        services
            .AddIdentity<AppIdentityDbContext>(options =>
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
            })
            .AddDefaultTokenProviders();

        // connect to AD
        var domainName = configuration.GetValue<string>("MemberOfDomain");
        if (!string.IsNullOrEmpty(domainName))
        {
            services.AddActiveDirectory(x => x.Name = domainName);
        }
        else
        {
            // fake service
            services.AddActiveDirectory();
        }
    }
}