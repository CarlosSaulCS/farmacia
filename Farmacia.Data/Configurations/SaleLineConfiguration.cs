using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class SaleLineConfiguration : IEntityTypeConfiguration<SaleLine>
{
    public void Configure(EntityTypeBuilder<SaleLine> builder)
    {
        builder.ToTable("venta_detalle");
        builder.Property(l => l.Quantity).HasColumnType("decimal(12,2)");
        builder.Property(l => l.UnitPrice).HasColumnType("decimal(12,2)");
        builder.Property(l => l.Discount).HasColumnType("decimal(12,2)");
        builder.Property(l => l.TaxRate).HasColumnType("decimal(5,2)");
        builder.Property(l => l.LineTotal).HasColumnType("decimal(12,2)");
        builder.HasOne(l => l.Product)
            .WithMany(p => p.SaleLines)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(l => l.ProductLot)
            .WithMany()
            .HasForeignKey(l => l.ProductLotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
