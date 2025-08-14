﻿using Monolith.Notifications;

namespace Monolith.Identity.Notifications;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request);

    Task<NotificationDto?> GetByIdAsync(string id);

    Task<int> CountUnreadAsync(string userId);

    Task SaveAsync(string fromUserId, string? fromName, string toUserId, SystemMessage message);

    Task ReadAsync(string id);

    Task ReadAllAsync(string userId);
}
