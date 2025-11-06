using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("productos");
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Presentation).HasMaxLength(120);
        builder.Property(p => p.InternalCode).HasMaxLength(50);
        builder.Property(p => p.Barcode).HasMaxLength(50);
        builder.Property(p => p.Location).HasMaxLength(80);
        builder.Property(p => p.TaxRate).HasColumnType("decimal(5,2)");
        builder.Property(p => p.Cost).HasColumnType("decimal(12,2)");
        builder.Property(p => p.Price).HasColumnType("decimal(12,2)");
        builder.Property(p => p.StockMinimum).HasColumnType("decimal(12,2)");
        builder.HasOne(p => p.Supplier)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(p => p.Barcode);
        builder.HasIndex(p => p.InternalCode);
    }
}
