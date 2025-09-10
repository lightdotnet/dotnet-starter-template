using Light.EntityFrameworkCore.Extensions;
using Light.Specification;
using Mapster;
using Monolith.Identity.Data;
using Monolith.Identity.Models;
using Monolith.Notifications;

namespace Monolith.Identity.Notifications;

internal class NotificationService(AppIdentityDbContext context) : INotificationService
{
    public Task<PagedResult<NotificationDto>> GetAsync(NotificationLookup request)
    {
        return context.Notifications
            .WhereIf(!string.IsNullOrEmpty(request.ToUser), x => x.ToUserId == request.ToUser)
            .WhereIf(request.OnlyUnread == true, x => x.ReadStatus == false)
            .AsNoTracking()
            .OrderByDescending(o => o.Created)
            .ProjectToType<NotificationDto>()
            .ToPagedResultAsync(request);
    }

    public Task<NotificationDto?> GetByIdAsync(string id)
    {
        return context.Notifications
            .Where(x => x.Id == id)
            .AsNoTracking()
            .ProjectToType<NotificationDto>()
            .SingleOrDefaultAsync();
    }

    public Task<int> CountUnreadAsync(string userId)
    {
        return context.Notifications
            .Where(x => x.ToUserId == userId && x.ReadStatus == false)
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

    public Task MarkAsReadAsync(string id)
    {
        return context.Notifications
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.ReadStatus, true));
    }

    public Task ReadAllAsync(string userId)
    {
        return context.Notifications
            .Where(x => x.ToUserId == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(p => p.ReadStatus, true));
    }
}
