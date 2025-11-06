using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class ProductLotConfiguration : IEntityTypeConfiguration<ProductLot>
{
    public void Configure(EntityTypeBuilder<ProductLot> builder)
    {
        builder.ToTable("lotes");
        builder.Property(l => l.LotCode).IsRequired().HasMaxLength(60);
        builder.Property(l => l.Quantity).HasColumnType("decimal(12,2)");
        builder.Property(l => l.RemainingQuantity).HasColumnType("decimal(12,2)");
        builder.HasOne(l => l.Product)
            .WithMany(p => p.Lots)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
