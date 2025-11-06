using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class PurchaseLineConfiguration : IEntityTypeConfiguration<PurchaseLine>
{
    public void Configure(EntityTypeBuilder<PurchaseLine> builder)
    {
        builder.ToTable("compra_detalle");
        builder.Property(l => l.Quantity).HasColumnType("decimal(12,2)");
        builder.Property(l => l.UnitCost).HasColumnType("decimal(12,2)");
        builder.Property(l => l.TaxRate).HasColumnType("decimal(5,2)");
        builder.HasOne(l => l.Product)
            .WithMany(p => p.PurchaseLines)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
