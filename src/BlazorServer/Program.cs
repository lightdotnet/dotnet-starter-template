using Light.FluentBlazor;
using Light.FluentBlazor.Settings;
using Light.Serilog;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Monolith;
using Monolith.BlazorServer;
using Monolith.BlazorServer.Components;
using Monolith.BlazorServer.Components.Account;
using Serilog;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Blazor Server").Color(Color.Purple));

StaticLogger.EnsureInitialized();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureSerilog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddFluentBlazorExtraComponents();
    builder.Services.AddFluentUIDemoServices();

    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();


    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.ConfigureServices(builder.Configuration);

    builder.Services.AddSingleton<IEmailSender<User>, IdentityNoOpEmailSender>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseStatusCodePagesWithRedirects("/");

    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.ConfigurePipelines();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    // Add additional endpoints required by the Identity /Account Razor components.
    app.MapAdditionalIdentityEndpoints();

    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("StopTheHostException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete.");
    Log.CloseAndFlush();
}

