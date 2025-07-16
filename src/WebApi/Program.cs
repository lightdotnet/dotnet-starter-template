using Light.Serilog;
using Monolith;
using Serilog;
using Spectre.Console;

AnsiConsole.Write(new FigletText("Starter API").Color(Color.Blue));

StaticLogger.EnsureInitialized();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureSerilog();

    // Add static Configuration to the container.

    // Add services to the container.
    builder.Services.ConfigureServices(builder.Configuration);

    builder.Services
        .AddLowercaseControllers()
        .AddDefaultJsonOptions()
        .AddInvalidModelStateHandler();

    var app = builder.Build();

    // Configure the HTTP request pipeline.

    app.UseHttpsRedirection();

    app.ConfigurePipelines();

    app.UseWebSockets();

    app.MapEndpoints();

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