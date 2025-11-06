using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.UI.Wpf.Services;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DashboardService _dashboardService;

    [ObservableProperty]
    private decimal _todaySales;

    [ObservableProperty]
    private int _todaySalesCount;

    public ObservableCollection<DashboardProduct> TopProducts { get; } = new();
    public ObservableCollection<DashboardAlert> Alerts { get; } = new();

    [ObservableProperty]
    private bool _isBusy;

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            IsBusy = true;
            var snapshot = await _dashboardService.GetSnapshotAsync(DateTime.Now);
            TodaySales = snapshot.TotalSales;
            TodaySalesCount = snapshot.SaleCount;

            TopProducts.Clear();
            foreach (var product in snapshot.TopProducts)
            {
                TopProducts.Add(product);
            }

            Alerts.Clear();
            foreach (var alert in snapshot.Alerts)
            {
                Alerts.Add(alert);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
