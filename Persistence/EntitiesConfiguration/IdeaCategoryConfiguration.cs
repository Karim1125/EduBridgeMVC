using EduBridgeMVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduBridgeMVC.Persistence.EntitiesConfiguration;

public class IdeaCategoryConfiguration : IEntityTypeConfiguration<IdeaCategory>
{
    public void Configure(EntityTypeBuilder<IdeaCategory> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}