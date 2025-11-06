namespace Farmacia.UI.Wpf.Models;

public class ProductPerformanceModel
{
    public required string ProductName { get; init; }
    public decimal QuantitySold { get; init; }
    public decimal Revenue { get; init; }
    public decimal CostOfGoods { get; init; }
    public decimal MarginImpact { get; init; }
    public decimal AverageDiscountPercent { get; init; }
    public decimal AverageUnitPrice { get; init; }

    public decimal MarginPercent => Revenue > 0 ? decimal.Round(MarginImpact / Revenue * 100m, 2) : 0m;
}
