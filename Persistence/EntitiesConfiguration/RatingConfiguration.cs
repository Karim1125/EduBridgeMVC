using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class RatingConfiguration : SoftDeleteConfiguration<Rating>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Rating> builder)
    {
        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId);

        builder.HasOne(x => x.Ta)
            .WithMany()
            .HasForeignKey(x => x.TaId);
    }
}