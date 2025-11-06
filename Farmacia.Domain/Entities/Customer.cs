namespace Farmacia.Domain.Entities;

public class Customer : EntityBase
{
    public required string Name { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Notes { get; set; }
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
