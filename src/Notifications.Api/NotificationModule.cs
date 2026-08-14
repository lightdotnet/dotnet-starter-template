using Light.AspNetCore.Authorization;
using Light.Extensions.DependencyInjection;
using Light.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Infrastructure.Modularity;
using StarterKit.Notifications.Api.Data;
using StarterKit.Notifications.Api.Services;
using StarterKit.Notifications.Contracts.Authorization;
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

        services.AddSingleton<IPermissionDefinitionProvider, NotificationPermissionProvider>();

        AddSmtpMail(services, configuration);

        ShowModuleInfo();
    }

    private static void AddSmtpMail(IServiceCollection services, IConfiguration configuration)
    {
        var smtpConfig = configuration.GetSection("SmtpMail").Get<SmtpMailKitOptions>()
            ?? throw new InvalidOperationException("SMTP configuration is missing.");

        services.AddSingleton(smtpConfig);
        
        services.AddSmtpMailKit(options =>
        {
            options.Host = smtpConfig.Host;
            options.Port = smtpConfig.Port;
            options.UserName = smtpConfig.UserName;
            options.Password = smtpConfig.Password;
            options.UseSsl = smtpConfig.UseSsl;
        });

        services.AddScoped<IMailService, MailService>();
    }
}