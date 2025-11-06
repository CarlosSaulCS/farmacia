namespace Farmacia.Domain.Entities;

public class PrescriptionItem : EntityBase
{
    public int ConsultationId { get; set; }
    public Consultation Consultation { get; set; } = null!;
    public string ProductName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Instructions { get; set; }
    public string? Duration { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
}
