namespace Farmacia.Domain.Entities;

public class ReturnLine : EntityBase
{
    public int ReturnOperationId { get; set; }
    public ReturnOperation ReturnOperation { get; set; } = null!;
    public int SaleLineId { get; set; }
    public SaleLine SaleLine { get; set; } = null!;
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}
