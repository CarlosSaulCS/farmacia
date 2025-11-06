using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

public class Appointment : EntityBase
{
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int DoctorId { get; set; }
    public User Doctor { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public TimeSpan Duration { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public int? ConsultationId { get; set; }
    public Consultation? Consultation { get; set; }
}
