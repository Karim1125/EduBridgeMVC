using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class TeamMemberConfiguration : SoftDeleteConfiguration<TeamMember>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TeamMember> builder)
    {
        builder.HasOne(x => x.Team)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TeamId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Teams)
            .HasForeignKey(x => x.UserId);
    }
}