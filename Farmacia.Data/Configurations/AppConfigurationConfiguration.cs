using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class AppConfigurationConfiguration : IEntityTypeConfiguration<AppConfiguration>
{
    public void Configure(EntityTypeBuilder<AppConfiguration> builder)
    {
        builder.ToTable("configuracion");
        builder.Property(c => c.Key).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Value).IsRequired().HasMaxLength(500);
        builder.HasIndex(c => c.Key).IsUnique();
    }
}
