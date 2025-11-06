using System;

namespace Farmacia.UI.Wpf.Models;

public class InventoryLotModel
{
    public string? LotCode { get; set; }
    public DateOnly? Expiration { get; set; }
    public decimal RemainingQuantity { get; set; }
}
