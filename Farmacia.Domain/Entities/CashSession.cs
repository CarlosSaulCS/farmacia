namespace Farmacia.Domain.Entities;

public class CashSession : EntityBase
{
    public required string SessionCode { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningAmount { get; set; }
    public decimal? ClosingAmount { get; set; }
    public int OpenedByUserId { get; set; }
    public User OpenedByUser { get; set; } = null!;
    public int? ClosedByUserId { get; set; }
    public User? ClosedByUser { get; set; }
    public bool IsZCut { get; set; }
    public ICollection<CashMovement> Movements { get; set; } = new List<CashMovement>();
}
