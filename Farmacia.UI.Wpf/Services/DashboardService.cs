using Farmacia.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.UI.Wpf.Services;

public class DashboardService
{
    private readonly PharmacyDbContext _context;

    public DashboardService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSnapshot> GetSnapshotAsync(DateTime asOf, CancellationToken cancellationToken = default)
    {
        var startOfDay = asOf.Date;
        var endOfDay = startOfDay.AddDays(1);

        var salesQuery = _context.Sales.AsNoTracking().Where(s => s.SaleDate >= startOfDay && s.SaleDate < endOfDay);
        var totalSales = await salesQuery.SumAsync(s => (decimal?)s.Total, cancellationToken) ?? 0m;
        var saleCount = await salesQuery.CountAsync(cancellationToken);

        var saleLines = await _context.SaleLines
            .AsNoTracking()
            .Include(l => l.Product)
            .Where(l => l.Sale.SaleDate >= startOfDay && l.Sale.SaleDate < endOfDay)
            .Select(l => new
            {
                ProductName = l.Product.Name,
                l.Quantity,
                l.LineTotal
            })
            .ToListAsync(cancellationToken);

        var topProducts = saleLines
            .GroupBy(l => l.ProductName)
            .Select(g => new DashboardProduct
            {
                Name = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                Total = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(p => p.Quantity)
            .ThenByDescending(p => p.Total)
            .Take(5)
            .ToList();

        var alerts = await _context.ProductLots
            .AsNoTracking()
            .Where(l => l.ExpirationDate != null && l.RemainingQuantity > 0 && l.ExpirationDate <= DateOnly.FromDateTime(asOf.AddDays(60)))
            .Select(l => new DashboardAlert
            {
                ProductName = l.Product.Name,
                LotCode = l.LotCode,
                ExpirationDate = l.ExpirationDate!.Value
            })
            .OrderBy(a => a.ExpirationDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new DashboardSnapshot(totalSales, saleCount, topProducts, alerts);
    }
}

public record DashboardSnapshot(decimal TotalSales, int SaleCount, IReadOnlyList<DashboardProduct> TopProducts, IReadOnlyList<DashboardAlert> Alerts);

public record DashboardProduct
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
}

public record DashboardAlert
{
    public string ProductName { get; set; } = string.Empty;
    public string LotCode { get; set; } = string.Empty;
    public DateOnly ExpirationDate { get; set; }
}
