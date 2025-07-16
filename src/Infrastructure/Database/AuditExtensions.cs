using Light.Domain.Entities.Interfaces;
using Light.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Monolith.Database;

public static class AuditExtensions
{
    public static void AuditEntries<TContext>(this TContext context, string? userId, DateTimeOffset auditTime, bool enableSoftDelete = false)
        where TContext : DbContext
    {
        var changeTracker = context.ChangeTracker;

        // fix null value when delete for Entities inherited ISoftDelete & ValueObjects
        changeTracker.Entries<ValueObject>()
            .Where(x => x.State is EntityState.Deleted)
            .ToList()
            .ForEach(e => e.State = EntityState.Unchanged);

        if (enableSoftDelete)
        {
            // auto set Deleted & DeletedBy for Entities inherited ISoftDelete
            changeTracker.Entries<ISoftDelete>()
                .Where(x => x.State is EntityState.Deleted)
                .ToList()
                .ForEach(e =>
                {
                    e.Entity.Deleted = auditTime;
                    e.Entity.DeletedBy = userId;
                    e.State = EntityState.Modified;
                });
        }

        // auto set LastModified for Entities inherited IModified
        changeTracker.Entries<IModified>()
            .Where(x => x.State is EntityState.Modified)
            .ToList()
            .ForEach(e =>
            {
                e.Entity.LastModified = auditTime;
                e.Entity.LastModifiedBy = userId;
            });

        // auto set Created & LastModified for Entities inherited ICreated
        changeTracker.Entries<ICreated>()
            .Where(x => x.State is EntityState.Added)
            .ToList()
            .ForEach(e =>
            {
                e.Entity.Created = auditTime;
                e.Entity.CreatedBy = userId;

                if (e.Entity is IModified modified)
                {
                    modified.LastModified = auditTime;
                    modified.LastModifiedBy = userId;
                }
            });
    }
}
