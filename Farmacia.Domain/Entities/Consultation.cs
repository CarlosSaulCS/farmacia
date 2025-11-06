namespace Farmacia.Domain.Entities;

public class Consultation : EntityBase
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public User Doctor { get; set; } = null!;
    public DateTime ConsultationDate { get; set; }
    public decimal? HeightMeters { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? HeartRate { get; set; }
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public string? ChiefComplaint { get; set; }
    public string? Diagnosis { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public string? PrescriptionFolio { get; set; }
    public ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
