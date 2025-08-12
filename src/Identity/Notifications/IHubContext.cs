using Monolith.Notifications;

namespace Monolith.Identity.Notifications;

public interface IHubContext
{
    Task SendAllAsync(INotificationMessage data, CancellationToken cancellationToken = default);

    Task SendAsync(string userId, INotificationMessage data, CancellationToken cancellationToken = default);

    Task SendAsync(IReadOnlyList<string> userIds, INotificationMessage data, CancellationToken cancellationToken = default);
}
