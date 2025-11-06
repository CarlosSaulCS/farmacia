namespace Farmacia.Domain.Entities;

public class ReturnOperation : EntityBase
{
    public required string Folio { get; set; }
    public DateTime ReturnDate { get; set; }
    public string? Reason { get; set; }
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public ICollection<ReturnLine> Lines { get; set; } = new List<ReturnLine>();
}
