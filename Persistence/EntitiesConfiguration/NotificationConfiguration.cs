using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class NotificationConfiguration : SoftDeleteConfiguration<Notification>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
}