namespace Monolith.Notifications;

public record SystemMessage : INotificationMessage
{
    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string? Url { get; set; }

    public bool ByMessage { get; set; } = false;
}
