namespace Farmacia.Domain.Entities;

public class ProductLot : EntityBase
{
    public required string LotCode { get; set; }
    public DateOnly? ExpirationDate { get; set; }
    public decimal Quantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
