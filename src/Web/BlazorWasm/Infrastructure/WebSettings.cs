using Monolith.Core;

namespace Monolith.Infrastructure;

public class WebSettings(IConfiguration configuration) : IWebSettings
{
    public string SignalRHub =>
        configuration.GetValue<string>("ApiUrls:SignalR_Hub")
        ?? throw new ArgumentNullException("SignalR_Hub");

    public string? Version => configuration.GetValue<string>("Version");
}
