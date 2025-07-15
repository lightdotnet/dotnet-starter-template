using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Monolith.Domain.Categories;

namespace Monolith.Infrastructure.Data.EntityConfigurations;

internal class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> entity)
    {
        entity.Property(p => p.SubOfId).HasMaxLength(450);
    }
}