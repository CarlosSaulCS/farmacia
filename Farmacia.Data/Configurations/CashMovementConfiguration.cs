using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class CashMovementConfiguration : IEntityTypeConfiguration<CashMovement>
{
    public void Configure(EntityTypeBuilder<CashMovement> builder)
    {
        builder.ToTable("movimientos_caja");
        builder.Property(m => m.Amount).HasColumnType("decimal(12,2)");
        builder.Property(m => m.Concept).HasMaxLength(200);
        builder.HasOne(m => m.CashSession)
            .WithMany(c => c.Movements)
            .HasForeignKey(m => m.CashSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
