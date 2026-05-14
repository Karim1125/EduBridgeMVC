using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class DoctorConfiguration : SoftDeleteConfiguration<Doctor>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(x => x.Department).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AcademicTitle).HasMaxLength(200);
        builder.Property(x => x.OfficeLocation).HasMaxLength(200);

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<Doctor>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}