using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("ventas");
        builder.Property(s => s.Folio).IsRequired().HasMaxLength(30);
        builder.Property(s => s.Subtotal).HasColumnType("decimal(12,2)");
        builder.Property(s => s.TaxTotal).HasColumnType("decimal(12,2)");
        builder.Property(s => s.Total).HasColumnType("decimal(12,2)");
        builder.Property(s => s.CashReceived).HasColumnType("decimal(12,2)");
        builder.Property(s => s.CardReceived).HasColumnType("decimal(12,2)");
        builder.Property(s => s.ChangeGiven).HasColumnType("decimal(12,2)");
        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(s => s.RelatedSale)
            .WithMany()
            .HasForeignKey(s => s.RelatedSaleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(s => s.Folio).IsUnique();
    }
}
