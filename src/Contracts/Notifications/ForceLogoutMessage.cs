namespace Monolith.Notifications;

public record ForceLogoutMessage(string UserId) : INotificationMessage;
