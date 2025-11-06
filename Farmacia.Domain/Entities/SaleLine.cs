namespace Farmacia.Domain.Entities;

public class SaleLine : EntityBase
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductLotId { get; set; }
    public ProductLot? ProductLot { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
