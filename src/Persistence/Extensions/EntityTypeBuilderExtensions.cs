using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StarterKit.Shared.Entities;

namespace StarterKit.Persistence.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static void ConfigureAuditableEntity<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(x => x.Id).HasMaxLength(450);

        builder.Property(x => x.CreatedBy).HasMaxLength(450);

        builder.Property(x => x.LastModifiedBy).HasMaxLength(450);
    }
}
