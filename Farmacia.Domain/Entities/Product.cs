namespace Farmacia.Domain.Entities;

public class Product : EntityBase
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Presentation { get; set; }
    public string? InternalCode { get; set; }
    public string? Barcode { get; set; }
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public decimal StockMinimum { get; set; }
    public string? Location { get; set; }
    public bool UsesBatches { get; set; }
    public ICollection<ProductLot> Lots { get; set; } = new List<ProductLot>();
    public ICollection<SaleLine> SaleLines { get; set; } = new List<SaleLine>();
    public ICollection<PurchaseLine> PurchaseLines { get; set; } = new List<PurchaseLine>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
