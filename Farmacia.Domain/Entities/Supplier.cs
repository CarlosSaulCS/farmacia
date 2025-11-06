namespace Farmacia.Domain.Entities;

public class Supplier : EntityBase
{
    public required string Name { get; set; }
    public string? Rfc { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
