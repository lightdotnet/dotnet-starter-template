namespace BlazorApp.Core;

public interface IWebSettings
{
    string SignalRHub { get; }

    string? Version { get; }
}
