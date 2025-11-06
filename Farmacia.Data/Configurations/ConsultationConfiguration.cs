using Farmacia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Farmacia.Data.Configurations;

public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
{
    public void Configure(EntityTypeBuilder<Consultation> builder)
    {
        builder.ToTable("consultas");
        builder.Property(c => c.ChiefComplaint).HasMaxLength(500);
        builder.Property(c => c.Diagnosis).HasMaxLength(500);
        builder.Property(c => c.TreatmentPlan).HasMaxLength(500);
        builder.Property(c => c.Notes).HasMaxLength(500);
        builder.Property(c => c.PrescriptionFolio).HasMaxLength(30);
        builder.HasIndex(c => c.PrescriptionFolio).IsUnique();
        builder.HasOne(c => c.Patient)
            .WithMany(p => p.Consultations)
            .HasForeignKey(c => c.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(c => c.Doctor)
            .WithMany()
            .HasForeignKey(c => c.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
