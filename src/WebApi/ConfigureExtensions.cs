﻿using Asp.Versioning.Conventions;
using FluentValidation;
using HealthChecks.UI.Client;
using Light.AspNetCore.Builder;
using Light.AspNetCore.Cors;
using Light.AspNetCore.Middlewares;
using Light.AspNetCore.Swagger;
using Light.Mediator;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Monolith.Identity;
using Monolith.Modularity;
using Monolith.Services;
using System.Reflection;

namespace Monolith;

public static class ConfigureExtensions
{
    private const string CORS_POLICY_NAME = "AllowCors";

    private static readonly Assembly[] assemblies =
        [
            typeof(Program).Assembly,
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
        services.AddModules<AppModule>(configuration, assemblies);

        var origins = configuration.GetSection("CorsOrigins").Get<string[]?>();
        if (origins is not null)
        {
            services.AddCors(opts => opts.AllowOrigins(CORS_POLICY_NAME, origins));
        }

        services.AddInfrastructureServices();
        services.AddHealthChecks();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, ServerCurrentUser>();
        services.AddPermissions();

        return services;
    }

    public static WebApplication ConfigurePipelines(this WebApplication app)
    {
        app.UseUlidTraceId()
            .UseLightRequestLogging()
            .UseLightExceptionHandler()
            .UseRouting()
            .UseCors(CORS_POLICY_NAME) // must add before Auth
            .UseAuthentication()
            .UseAuthorization()
            .UseSwagger();

        app.UseModules<AppModule>(assemblies);

        app.MapHealthChecks("/hc", new HealthCheckOptions()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapModuleEndpoints<AppHub>(assemblies);

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

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder builder)
    {
        var isDebug = false;

#if DEBUG
        isDebug = true;
#endif

        if (isDebug)
        {
            builder.MapControllers().AllowAnonymous();
        }
        else
        {
            builder.MapControllers().RequireAuthorization();
        }

        return builder;
    }
}