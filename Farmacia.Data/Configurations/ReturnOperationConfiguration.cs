using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class ReturnOperationConfiguration : IEntityTypeConfiguration<ReturnOperation>
{
    public void Configure(EntityTypeBuilder<ReturnOperation> builder)
    {
        builder.ToTable("devoluciones");
        builder.Property(r => r.Folio).IsRequired().HasMaxLength(30);
        builder.HasIndex(r => r.Folio).IsUnique();
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
