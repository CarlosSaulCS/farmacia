using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

public class InventoryMovement : EntityBase
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int? ProductLotId { get; set; }
    public ProductLot? ProductLot { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
