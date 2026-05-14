using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class TaRequestConfiguration : SoftDeleteConfiguration<TaRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TaRequest> builder)
    {
        builder.HasOne(x => x.Team)
            .WithMany(x => x.TARequests)
            .HasForeignKey(x => x.TeamId);

        builder.HasOne(x => x.TA)
            .WithMany()
            .HasForeignKey(x => x.TAId);
    }
}