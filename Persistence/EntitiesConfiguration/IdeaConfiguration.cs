using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class IdeaConfiguration : SoftDeleteConfiguration<Idea>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Idea> builder)
    {
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(3000).IsRequired();
        builder.Property(x => x.RepositoryUrl).HasMaxLength(500);

        builder.HasOne(x => x.Team)
            .WithOne(x => x.Idea)
            .HasForeignKey<Idea>(x => x.TeamId);
    }
}