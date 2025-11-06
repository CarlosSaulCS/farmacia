using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class SequenceConfiguration : IEntityTypeConfiguration<Sequence>
{
    public void Configure(EntityTypeBuilder<Sequence> builder)
    {
        builder.ToTable("secuencias");
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Prefix).HasMaxLength(10);
    }
}
