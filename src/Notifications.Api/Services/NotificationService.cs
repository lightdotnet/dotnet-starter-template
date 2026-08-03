using Light.EntityFrameworkCore.Extensions;
using Light.Specification;
using Mapster;
using Microsoft.EntityFrameworkCore;
using StarterKit.Notifications.Api.Data;
using StarterKit.Notifications.Api.Entities;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Notifications.Contracts.SystemNotifications;
using StarterKit.Persistence.Extensions;

namespace StarterKit.Notifications.Api.Services;

internal class NotificationService(
    NotificationDbContext context) : INotificationService
{
    public Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request)
    {
        return context.Notifications
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(request.ToUserId), x => x.ToUserId == request.ToUserId)
            .WhereIf(request.Status.HasValue, x => x.Status == request.Status!.Value)
            .OrderByDescending(o => o.Created)
            .ProjectToType<NotificationDto>()
            .ToPagedResultAsync(request);
    }

    public Task<NotificationDto?> GetByIdAsync(string userId, string id)
    {
        return context.Notifications
            .AsNoTracking()
            .Where(x => x.Id == id && x.ToUserId == userId)
            .ProjectToType<NotificationDto>()
            .SingleOrDefaultAsync();
    }

    public Task<int> CountUnreadAsync(string userId)
    {
        return context.Notifications
            .Where(x => x.ToUserId == userId && x.Status == NotificationStatus.None)
            .CountAsync();
    }

    public async Task SaveAsync(string fromUserId, string? fromName, string toUserId, SystemMessage message)
    {
        var entity = new Notification
        {
            FromUserId = fromUserId,
            FromName = fromName,
            ToUserId = toUserId,
            Title = message.Title,
            Message = message.Message,
            Url = message.Url
        };

        await context.Notifications.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public Task MarkAsReadAsync(string userId, string id)
    {
        return context.Notifications
            .Where(x => x.Id == id && x.ToUserId == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, NotificationStatus.Read));
    }

    public Task ReadAllAsync(string userId)
    {
        return context.Notifications
            .Where(x => x.ToUserId == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, NotificationStatus.Read));
    }
}
