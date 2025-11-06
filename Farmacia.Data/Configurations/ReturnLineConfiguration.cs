using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class ReturnLineConfiguration : IEntityTypeConfiguration<ReturnLine>
{
    public void Configure(EntityTypeBuilder<ReturnLine> builder)
    {
        builder.ToTable("devolucion_detalle");
        builder.Property(l => l.Quantity).HasColumnType("decimal(12,2)");
        builder.HasOne(l => l.SaleLine)
            .WithMany()
            .HasForeignKey(l => l.SaleLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
