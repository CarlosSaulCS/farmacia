using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("cortes_caja");
        builder.Property(c => c.SessionCode).IsRequired().HasMaxLength(30);
        builder.Property(c => c.OpeningAmount).HasColumnType("decimal(12,2)");
        builder.Property(c => c.ClosingAmount).HasColumnType("decimal(12,2)");
        builder.HasOne(c => c.OpenedByUser)
            .WithMany()
            .HasForeignKey(c => c.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(c => c.ClosedByUser)
            .WithMany()
            .HasForeignKey(c => c.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => c.SessionCode).IsUnique();
    }
}
