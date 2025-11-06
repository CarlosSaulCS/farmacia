namespace Farmacia.Domain.Services;

public interface IReportingService
{
    Task<byte[]> BuildSalesSummaryPdfAsync(DateOnly startDate, DateOnly endDate, string requestedBy, CancellationToken cancellationToken = default);
    Task<byte[]> BuildInventoryCsvAsync(DateOnly cutoffDate, string requestedBy, CancellationToken cancellationToken = default);
    Task<SalesMetrics> GetSalesMetricsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SalesTrendPoint>> GetSalesTrendAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductPerformance>> GetTopProductsAsync(DateOnly startDate, DateOnly endDate, int take = 6, CancellationToken cancellationToken = default);
    Task<PricingInsights> GetPricingInsightsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);
}

public record SalesMetrics(decimal TotalSales, int SaleCount, decimal AverageTicket);

public record SalesTrendPoint(DateOnly Date, decimal TotalSales, int TicketCount, decimal AverageTicket);

public record ProductPerformance(string ProductName, decimal QuantitySold, decimal Revenue, decimal CostOfGoods, decimal MarginImpact, decimal AverageDiscountPercent, decimal AverageUnitPrice);

public record PricingInsights(decimal TotalRevenue, decimal TotalCost, decimal GrossMargin, decimal GrossMarginPercent, decimal AverageUnitMargin, decimal AverageUnitPrice, decimal AverageUnitCost, decimal AverageDiscountPercent, decimal PriceSpread);
