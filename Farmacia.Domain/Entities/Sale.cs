using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

public class Sale : EntityBase
{
    public required string Folio { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal Total { get; set; }
    public decimal? CashReceived { get; set; }
    public decimal? CardReceived { get; set; }
    public decimal? ChangeGiven { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public string? Notes { get; set; }
    public int? RelatedSaleId { get; set; }
    public Sale? RelatedSale { get; set; }
    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
    public ICollection<ReturnOperation> Returns { get; set; } = new List<ReturnOperation>();
}
