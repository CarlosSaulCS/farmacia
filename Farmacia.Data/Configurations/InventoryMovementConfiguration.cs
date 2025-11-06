using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.ToTable("movimientos_inventario");
        builder.Property(m => m.Quantity).HasColumnType("decimal(12,2)");
        builder.Property(m => m.Reason).HasMaxLength(200);
        builder.HasOne(m => m.Product)
            .WithMany(p => p.InventoryMovements)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(m => m.ProductLot)
            .WithMany()
            .HasForeignKey(m => m.ProductLotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
