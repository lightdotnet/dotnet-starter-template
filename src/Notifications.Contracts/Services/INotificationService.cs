using StarterKit.Notifications.Contracts.SystemNotifications;

namespace StarterKit.Notifications.Contracts.Services;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request);

    Task<NotificationDto?> GetByIdAsync(string userId, string id);

    Task<int> CountUnreadAsync(string userId);

    Task MarkAsReadAsync(string userId, string id);

    Task ReadAllAsync(string userId);

    Task SaveAsync(string fromUserId, string? fromName, string toUserId, SystemMessage message);
}
