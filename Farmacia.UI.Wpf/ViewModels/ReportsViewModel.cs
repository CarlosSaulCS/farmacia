using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Domain.Services;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.Services;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly IReportingService _reportingService;
    private readonly UserSessionService _sessionService;

    [ObservableProperty]
    private DateOnly _startDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty]
    private DateOnly _endDate = DateOnly.FromDateTime(DateTime.Today);

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    [ObservableProperty]
    private decimal _metricsTotalSales;

    [ObservableProperty]
    private int _metricsSaleCount;

    [ObservableProperty]
    private decimal _metricsAverageTicket;

    [ObservableProperty]
    private string _requestedBy = "Sin usuario";

    [ObservableProperty]
    private PricingInsightsModel? _pricingInsights;

    public ObservableCollection<SalesTrendPointModel> SalesTrendPoints { get; } = new();
    public ObservableCollection<ProductPerformanceModel> TopProducts { get; } = new();

    public bool CanExport => !IsBusy;

    public ReportsViewModel(IReportingService reportingService, UserSessionService sessionService)
    {
        _reportingService = reportingService;
        _sessionService = sessionService;
        _sessionService.SessionChanged += (_, session) => UpdateRequestedBy(session);

        UpdateRequestedBy(_sessionService.Current);
        _ = RefreshMetricsCommand.ExecuteAsync(null);
    }

    partial void OnIsBusyChanged(bool value)
    {
        ExportSalesSummaryCommand.NotifyCanExecuteChanged();
        ExportInventoryCommand.NotifyCanExecuteChanged();
        RefreshMetricsCommand.NotifyCanExecuteChanged();
    }

    partial void OnStartDateChanged(DateOnly value)
    {
        ScheduleMetricsRefresh();
    }

    partial void OnEndDateChanged(DateOnly value)
    {
        ScheduleMetricsRefresh();
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportSalesSummaryAsync()
    {
        if (EndDate < StartDate)
        {
            StatusMessage = "La fecha final debe ser posterior a la inicial.";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Archivo PDF (*.pdf)|*.pdf",
            FileName = $"ventas-{StartDate:yyyyMMdd}-{EndDate:yyyyMMdd}.pdf",
            AddExtension = true, DefaultExt = ".pdf"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var requester = RequestedBy;
            var data = await _reportingService.BuildSalesSummaryPdfAsync(StartDate, EndDate, requester);
            await File.WriteAllBytesAsync(dialog.FileName, data);
            StatusMessage = $"Reporte de ventas exportado por {requester}.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportInventoryAsync()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Archivo CSV (*.csv)|*.csv",
            FileName = $"inventario-{DateTime.Today:yyyyMMdd}.csv",
            AddExtension = true, DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            IsBusy = true;
            var requester = RequestedBy;
            var data = await _reportingService.BuildInventoryCsvAsync(DateOnly.FromDateTime(DateTime.Today), requester);
            await File.WriteAllBytesAsync(dialog.FileName, data);
            StatusMessage = $"Inventario exportado por {requester}.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task RefreshMetricsAsync()
    {
        if (!ValidateRange())
        {
            return;
        }

        try
        {
            IsBusy = true;
            var metrics = await _reportingService.GetSalesMetricsAsync(StartDate, EndDate);
            MetricsTotalSales = metrics.TotalSales;
            MetricsSaleCount = metrics.SaleCount;
            MetricsAverageTicket = metrics.AverageTicket;

            await LoadAnalyticsAsync();
            if (!SalesTrendPoints.Any() && !TopProducts.Any())
            {
                StatusMessage = "No se encontraron datos en el periodo seleccionado.";
            }
            else
            {
                StatusMessage = "Indicadores actualizados correctamente.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateRange()
    {
        if (EndDate < StartDate)
        {
            StatusMessage = "La fecha final debe ser posterior a la inicial.";
            return false;
        }

        StatusMessage = null;
        return true;
    }

    private void UpdateRequestedBy(UserSession? session)
    {
        RequestedBy = session?.FullName ?? "Sin usuario";
    }

    private void ScheduleMetricsRefresh()
    {
        if (IsBusy)
        {
            return;
        }

        _ = RefreshMetricsCommand.ExecuteAsync(null);
    }

    private async Task LoadAnalyticsAsync()
    {
        SalesTrendPoints.Clear();
        TopProducts.Clear();
        PricingInsights = null;

        var trendTask = _reportingService.GetSalesTrendAsync(StartDate, EndDate);
        var productsTask = _reportingService.GetTopProductsAsync(StartDate, EndDate, 8);
        var pricingTask = _reportingService.GetPricingInsightsAsync(StartDate, EndDate);

        await Task.WhenAll(trendTask, productsTask, pricingTask);

    var trend = await trendTask;
        foreach (var point in trend)
        {
            SalesTrendPoints.Add(new SalesTrendPointModel
            {
                Date = point.Date,
                TotalSales = point.TotalSales,
                TicketCount = point.TicketCount,
                AverageTicket = point.AverageTicket
            });
        }

    var products = await productsTask;
        foreach (var product in products)
        {
            TopProducts.Add(new ProductPerformanceModel
            {
                ProductName = product.ProductName,
                QuantitySold = product.QuantitySold,
                Revenue = product.Revenue,
                CostOfGoods = product.CostOfGoods,
                MarginImpact = product.MarginImpact,
                AverageDiscountPercent = product.AverageDiscountPercent,
                AverageUnitPrice = product.AverageUnitPrice
            });
        }

    var insights = await pricingTask;
        PricingInsights = new PricingInsightsModel
        {
            TotalRevenue = insights.TotalRevenue,
            TotalCost = insights.TotalCost,
            GrossMargin = insights.GrossMargin,
            GrossMarginPercent = insights.GrossMarginPercent,
            AverageUnitMargin = insights.AverageUnitMargin,
            AverageUnitPrice = insights.AverageUnitPrice,
            AverageUnitCost = insights.AverageUnitCost,
            AverageDiscountPercent = insights.AverageDiscountPercent,
            PriceSpread = insights.PriceSpread
        };
    }
}
