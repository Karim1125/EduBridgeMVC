using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class DoctorRequestConfiguration : SoftDeleteConfiguration<DoctorRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<DoctorRequest> builder)
    {
        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.DoctorRequests)
            .HasForeignKey(x => x.DoctorId);
    }
}