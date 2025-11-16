using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Monolith.Notifications;

namespace Monolith.Identity.Notifications.SignalR;

internal class HubService : IHubService
{
    private readonly IHubContext<SignalRHub> _notificationHubContext;
    private readonly ILogger<HubService> _logger;

    public HubService(IHubContext<SignalRHub> notificationHubContext, ILogger<HubService> logger) =>
        (_notificationHubContext, _logger) = (notificationHubContext, logger);

    public Task NotifyAsync(CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.All.SendAsync(
            NotificationConstants.SERVER_NOTIFICATION, cancellationToken);

    public Task NotifyAsync(string userId, CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.User(userId).SendAsync(
            NotificationConstants.SERVER_NOTIFICATION, cancellationToken);

    public Task NotifyAsync(IEnumerable<string> userIds,
        CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.Users(userIds).SendAsync(
            NotificationConstants.SERVER_NOTIFICATION, cancellationToken);

    public Task SendAsync<T>(T data, CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.All.SendAsync(typeof(T).Name, data, cancellationToken);

    public Task SendAsync<T>(T data, string userId, CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.User(userId).SendAsync(typeof(T).Name, data, cancellationToken);

    public Task SendAsync<T>(T data, IEnumerable<string> userIds, CancellationToken cancellationToken = default) =>
        _notificationHubContext.Clients.Users(userIds).SendAsync(typeof(T).Name, data, cancellationToken);
}
