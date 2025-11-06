using System;
using System.Text;
using Farmacia.Data.Contexts;
using Farmacia.Domain.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Farmacia.UI.Wpf.Services;

public class ReportingService : IReportingService
{
    private readonly PharmacyDbContext _context;

    public ReportingService(PharmacyDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> BuildSalesSummaryPdfAsync(DateOnly startDate, DateOnly endDate, string requestedBy, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var data = await _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= start && s.SaleDate <= end)
            .Select(s => new { s.Folio, s.SaleDate, s.Total, User = s.User.FullName })
            .OrderBy(s => s.SaleDate)
            .ToListAsync(cancellationToken);

        var total = data.Sum(s => s.Total);

        var generatedAt = DateTime.Now;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text($"Ventas del {startDate:dd/MM/yyyy} al {endDate:dd/MM/yyyy}").FontSize(18).SemiBold();
                    column.Item().Text($"Generado por: {requestedBy} el {generatedAt:dd/MM/yyyy HH:mm}").FontSize(10).Italic();
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(80);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Folio").SemiBold();
                        header.Cell().Text("Fecha").SemiBold();
                        header.Cell().Text("Usuario").SemiBold();
                        header.Cell().AlignRight().Text("Total").SemiBold();
                    });

                    foreach (var sale in data)
                    {
                        table.Cell().Text(sale.Folio);
                        table.Cell().Text(sale.SaleDate.ToString("dd/MM/yyyy HH:mm"));
                        table.Cell().Text(sale.User);
                        table.Cell().AlignRight().Text(sale.Total.ToString("C2"));
                    }

                    table.Footer(footer =>
                    {
                        footer.Cell().ColumnSpan(3).Text("Total").SemiBold();
                        footer.Cell().AlignRight().Text(total.ToString("C2")).SemiBold();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<byte[]> BuildInventoryCsvAsync(DateOnly cutoffDate, string requestedBy, CancellationToken cancellationToken = default)
    {
        var data = await _context.ProductLots
            .AsNoTracking()
            .Where(l => l.RemainingQuantity > 0)
            .Select(l => new
            {
                l.Product.Name,
                l.LotCode,
                Expiration = l.ExpirationDate,
                l.RemainingQuantity
            })
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine($"Generado por,{Escape(requestedBy)},Fecha,{DateTime.Now:dd/MM/yyyy HH:mm}");
        builder.AppendLine("Producto,Lote,Caducidad,Cantidad");
        foreach (var row in data)
        {
            var expiration = row.Expiration?.ToString("dd/MM/yyyy") ?? "";
            builder.AppendLine($"{Escape(row.Name)},{Escape(row.LotCode)},{expiration},{row.RemainingQuantity}");
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public async Task<SalesMetrics> GetSalesMetricsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var query = _context.Sales.AsNoTracking().Where(s => s.SaleDate >= start && s.SaleDate <= end);
        var total = await query.SumAsync(s => (decimal?)s.Total, cancellationToken) ?? 0m;
        var count = await query.CountAsync(cancellationToken);
        var average = count > 0 ? Math.Round(total / count, 2) : 0m;
        return new SalesMetrics(total, count, average);
    }

    public async Task<IReadOnlyList<SalesTrendPoint>> GetSalesTrendAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var data = await _context.Sales
            .AsNoTracking()
            .Where(s => s.SaleDate >= start && s.SaleDate <= end)
            .GroupBy(s => s.SaleDate.Date)
            .Select(group => new
            {
                Date = group.Key,
                Total = group.Sum(x => x.Total),
                Count = group.Count()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return data
            .Select(entry =>
            {
                var average = entry.Count > 0 ? Math.Round(entry.Total / entry.Count, 2) : 0m;
                return new SalesTrendPoint(DateOnly.FromDateTime(entry.Date), entry.Total, entry.Count, average);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ProductPerformance>> GetTopProductsAsync(DateOnly startDate, DateOnly endDate, int take = 6, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);
        var limit = Math.Clamp(take, 1, 20);

        var aggregates = await _context.SaleLines
            .AsNoTracking()
            .Where(line => line.Sale.SaleDate >= start && line.Sale.SaleDate <= end)
            .GroupBy(line => new { line.ProductId, line.Product.Name })
            .Select(group => new
            {
                group.Key.Name,
                Quantity = group.Sum(line => line.Quantity),
                Revenue = group.Sum(line => line.LineTotal),
                Cost = group.Sum(line => line.Quantity * line.Product.Cost),
                DiscountBase = group.Sum(line => line.Quantity * line.UnitPrice),
                Discount = group.Sum(line => line.Discount)
            })
            .ToListAsync(cancellationToken);

        var ordered = aggregates
            .OrderByDescending(x => x.Revenue)
            .ThenByDescending(x => x.Quantity)
            .Take(limit)
            .ToList();

        var performances = ordered.Select(item =>
        {
            var margin = item.Revenue - item.Cost;
            var averageDiscount = item.DiscountBase > 0 ? Math.Round(item.Discount / item.DiscountBase * 100m, 2) : 0m;
            var averageUnitPrice = item.Quantity > 0 ? Math.Round(item.Revenue / item.Quantity, 2) : 0m;
            return new ProductPerformance(item.Name, item.Quantity, item.Revenue, item.Cost, margin, averageDiscount, averageUnitPrice);
        }).ToList();

        return performances;
    }

    public async Task<PricingInsights> GetPricingInsightsAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default)
    {
        var start = startDate.ToDateTime(TimeOnly.MinValue);
        var end = endDate.ToDateTime(TimeOnly.MaxValue);

        var lines = await _context.SaleLines
            .AsNoTracking()
            .Where(line => line.Sale.SaleDate >= start && line.Sale.SaleDate <= end)
            .Select(line => new
            {
                line.LineTotal,
                line.Quantity,
                line.UnitPrice,
                line.Discount,
                Cost = line.Quantity * line.Product.Cost
            })
            .ToListAsync(cancellationToken);

        if (lines.Count == 0)
        {
            return new PricingInsights(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
        }

        var totalRevenue = lines.Sum(x => x.LineTotal);
        var totalCost = lines.Sum(x => x.Cost);
        var grossMargin = totalRevenue - totalCost;
        var grossMarginPercent = totalRevenue > 0m ? Math.Round(grossMargin / totalRevenue * 100m, 2) : 0m;
        var totalQuantity = lines.Sum(x => x.Quantity);
        var averageUnitMargin = totalQuantity > 0m ? Math.Round(grossMargin / totalQuantity, 2) : 0m;
        var averageUnitPrice = totalQuantity > 0m ? Math.Round(totalRevenue / totalQuantity, 2) : 0m;
        var averageUnitCost = totalQuantity > 0m ? Math.Round(totalCost / totalQuantity, 2) : 0m;
        var discountBase = lines.Sum(x => x.Quantity * x.UnitPrice);
        var averageDiscountPercent = discountBase > 0m ? Math.Round(lines.Sum(x => x.Discount) / discountBase * 100m, 2) : 0m;
        var priceSpread = lines.Count > 0 ? Math.Round(lines.Max(x => x.UnitPrice) - lines.Min(x => x.UnitPrice), 2) : 0m;

        return new PricingInsights(
            totalRevenue,
            totalCost,
            grossMargin,
            grossMarginPercent,
            averageUnitMargin,
            averageUnitPrice,
            averageUnitCost,
            averageDiscountPercent,
            priceSpread);
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
