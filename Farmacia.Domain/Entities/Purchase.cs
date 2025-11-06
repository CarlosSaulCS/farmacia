namespace Farmacia.Domain.Entities;

public class Purchase : EntityBase
{
    public required string Folio { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal Total { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<PurchaseLine> Lines { get; set; } = new List<PurchaseLine>();
}
