using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class JoinRequestConfiguration : SoftDeleteConfiguration<JoinRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<JoinRequest> builder)
    {
        builder.HasOne(x => x.Team)
            .WithMany(x => x.JoinRequests)
            .HasForeignKey(x => x.TeamId);

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentId);
    }
}