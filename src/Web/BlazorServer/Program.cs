using Light.FluentBlazor;
using Light.FluentBlazor.Settings;
using Light.Serilog;
using Monolith;
using Monolith.Components;
using Serilog;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Blazor Server").Color(Color.Purple));

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
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();

        app.UseStatusCodePagesWithRedirects("/Error");
    }
    else
    {
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
    AppLogging.Logger.Fatal("Unhandled exception: {ex}", ex);
}
finally
{
    Log.Information("Shut down complete.");
    Log.CloseAndFlush();
}

