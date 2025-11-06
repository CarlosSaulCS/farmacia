namespace Farmacia.Domain.Entities;

public class CashMovement : EntityBase
{
    public int CashSessionId { get; set; }
    public CashSession CashSession { get; set; } = null!;
    public DateTime MovementDate { get; set; }
    public decimal Amount { get; set; }
    public string Concept { get; set; } = string.Empty;
    public bool IsIncome { get; set; }
}
