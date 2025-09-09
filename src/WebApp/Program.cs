using Light.FluentBlazor;
using Light.FluentBlazor.Settings;
using Light.Serilog;
using Monolith;
using Monolith.WebAdmin;
using Monolith.WebAdmin.Components;
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

    builder.Services.ConfigureServices(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {

    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();

        app.UseStatusCodePagesWithRedirects("/");
    }

    app.UseHttpsRedirection();

    app.UseAntiforgery();

    app.ConfigurePipelines();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

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

