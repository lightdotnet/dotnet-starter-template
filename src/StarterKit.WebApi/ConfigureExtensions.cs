using Asp.Versioning.Conventions;
using FluentValidation;
using Light.AspNetCore.Builder;
using Light.AspNetCore.Middlewares;
using Light.AspNetCore.Swagger;
using Light.Mediator;
using StarterKit.Identity.Api;
using StarterKit.Infrastructure;
using StarterKit.Infrastructure.Cors;
using StarterKit.Infrastructure.HealthChecks;
using StarterKit.Infrastructure.Modularity;
using StarterKit.Infrastructure.Services;
using StarterKit.Shared;
using StarterKit.Shared.Authorization;
using System.Reflection;

namespace StarterKit.WebApi;

public static class ConfigureExtensions
{
    private static readonly Assembly[] assemblies =
        [
            typeof(Program).Assembly, // inject this to import Identity Module
            typeof(IdentityModule).Assembly,
        ];

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssemblies(assemblies);

        // Light Framework
        services.AddMediatorFromAssemblies(assemblies);
        services.AddBehaviors(typeof(ValidationBehaviour<,>));
        services.AddOptions<RequestLoggingOptions>().BindConfiguration("RequestLogging");
        services.AddGlobalExceptionHandler();
        services.AddApiVersion(1);
        services.AddSwagger(configuration);
        services.AddFileGenerator();

        services.AddSharedInfrastructure();
        services.AddHealthChecksService();
        services.AddCorsPolicy(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissionPolicies();
        services.AddPermissionAuthorization();

        services.AddModules<AppModule>(configuration, assemblies);

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        app
            .UseUlidTraceId()
            .UseLightRequestLogging()
            .UseLightExceptionHandler()
            .UseRouting()
            .UseCorsPolicy() // must add before Auth
            .UseAuthentication()
            .UseAuthorization()
            .UseSwagger();

        app.MapHealthChecksEndpoint();

        app.UseModules<AppModule>(assemblies);

        app.MapModuleEndpoints<AppModuleEndpoint>(assemblies);

        //register api versions
        var versions = app
            .NewApiVersionSet()
            .HasApiVersion(1)
            .ReportApiVersions()
            .Build();

        //map versioned endpoint
        var endpoints = app.MapGroup("api/v{version:apiVersion}").WithApiVersionSet(versions);
        endpoints.MapModuleEndpoints<AppModule>(assemblies);

        return app;
    }
}