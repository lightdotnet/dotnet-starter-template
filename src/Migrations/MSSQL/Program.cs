using Microsoft.Extensions.DependencyInjection;
using MSSQL;
using StarterKit.Notifications.Api.Data;

// set Environment
//Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Live");

using var host = Host.CreateHostBuilder(args).Build();

using var scope = host.Services.CreateScope();

var serviceProvider = scope.ServiceProvider;

// Identity module
var identityInitialiser = serviceProvider.GetRequiredService<IdentityContextInitialiser>();

await identityInitialiser.InitialiseAsync();

await identityInitialiser.TrySeedAsync();

// Notification module
var notificationInitialiser = serviceProvider.GetRequiredService<NotificationContextInitialiser>();

await notificationInitialiser.InitialiseAsync();

// Organization module
var organizationInitialiser = serviceProvider.GetRequiredService<OrganizationContextInitialiser>();

await organizationInitialiser.InitialiseAsync();

await organizationInitialiser.TrySeedAsync();