using System;

namespace Farmacia.UI.Wpf.Models;

public class PatientListItemModel
{
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public DateTime? LastConsultation { get; set; }
    public string? Notes { get; set; }
    public bool HasPhone => !string.IsNullOrWhiteSpace(Phone);
    public bool IsBirthdaySoon { get; set; }

    public int? Age => BirthDate.HasValue
        ? (int)Math.Floor((DateTime.Today - BirthDate.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25)
        : null;
}
