namespace Farmacia.Domain.Entities;

public class PurchaseLine : EntityBase
{
    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string? LotCode { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxRate { get; set; }
}
