using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class IdeaTagConfiguration : SoftDeleteConfiguration<IdeaTag>
{
    protected override void ConfigureEntity(EntityTypeBuilder<IdeaTag> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Tags)
            .HasForeignKey(x => x.CategoryId);

        builder.HasIndex(x => new { x.Name, x.CategoryId }).IsUnique();
    }
}