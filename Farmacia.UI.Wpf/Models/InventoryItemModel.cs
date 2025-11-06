using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmacia.UI.Wpf.Models;

public class InventoryItemModel
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Supplier { get; set; }
    public decimal Cost { get; set; }
    public decimal Price { get; set; }
    public decimal TaxRate { get; set; }
    public bool UsesBatches { get; set; }
    public decimal Stock { get; set; }
    public decimal Minimum { get; set; }
    public bool IsBelowMinimum => Minimum > 0 && Stock < Minimum;
    public DateOnly? ClosestExpiration { get; set; }
    public bool HasLots => Lots.Any();
    public bool IsExpiringSoon { get; set; }
    public List<InventoryLotModel> Lots { get; set; } = new();
    public decimal TaxRatePercentage => Math.Round(TaxRate * 100m, 2);
}
