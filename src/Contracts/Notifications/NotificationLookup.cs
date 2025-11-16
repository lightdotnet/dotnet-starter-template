namespace Monolith.Notifications;

public record NotificationLookup : IPage
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? ToUserId { get; set; }

    public bool OnlyUnread { get; set; } = false;
}