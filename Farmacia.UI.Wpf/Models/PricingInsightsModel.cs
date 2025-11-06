namespace Farmacia.UI.Wpf.Models;

public class PricingInsightsModel
{
    public decimal TotalRevenue { get; init; }
    public decimal TotalCost { get; init; }
    public decimal GrossMargin { get; init; }
    public decimal GrossMarginPercent { get; init; }
    public decimal AverageUnitMargin { get; init; }
    public decimal AverageUnitPrice { get; init; }
    public decimal AverageUnitCost { get; init; }
    public decimal AverageDiscountPercent { get; init; }
    public decimal PriceSpread { get; init; }
}
