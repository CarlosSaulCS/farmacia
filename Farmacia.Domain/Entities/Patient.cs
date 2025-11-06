namespace Farmacia.Domain.Entities;

public class Patient : EntityBase
{
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Notes { get; set; }
    public string? GeneralData { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
