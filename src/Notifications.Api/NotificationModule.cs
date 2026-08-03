using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Infrastructure.Modularity;
using StarterKit.Notifications.Api.Data;
using StarterKit.Notifications.Api.Services;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Persistence;

namespace StarterKit.Notifications.Api;

public class NotificationModule : AppModule
{
    public override void Add(IServiceCollection services, IConfiguration configuration)
    {
        services.AddConfiguredDbContext<NotificationDbContext>(
            configuration,
            DbConnectionNames.Identity);

        services.AddScoped<INotificationService, NotificationService>();

        ShowModuleInfo();
    }
}