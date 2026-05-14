using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Bio).HasMaxLength(1000);
        builder.Property(x => x.Major).HasMaxLength(200);
        builder.Property(x => x.University).HasMaxLength(200);
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(500);
        builder.Property(x => x.GitHubUrl).HasMaxLength(500);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(500);
        builder.Property(x => x.IsDisabled).HasDefaultValue(false);

        builder.HasMany(x => x.Skills)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.HasMany(x => x.Teams)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId);

        builder.OwnsMany(x => x.RefreshTokens, rt =>
        {
            rt.Property(x => x.Token).HasMaxLength(500).IsRequired();
            rt.HasIndex(x => x.Token).IsUnique();
        });
    }
}