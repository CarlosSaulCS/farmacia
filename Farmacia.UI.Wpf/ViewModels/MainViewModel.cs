using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.Services;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavigationService _navigationService;
    private readonly UserSessionService _sessionService;

    public ObservableCollection<NavigationItem> NavigationItems { get; } = new();

    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    [ObservableProperty]
    private NavigationItem? _selectedNavigation;

    public MainViewModel(NavigationService navigationService, UserSessionService sessionService)
    {
        _navigationService = navigationService;
        _sessionService = sessionService;
        _navigationService.CurrentViewChanged += (_, view) => CurrentView = view;
        _sessionService.SessionChanged += (_, session) => UpdateWelcomeMessage(session);

        UpdateWelcomeMessage(_sessionService.Current);

        NavigationItems.Add(CreateNavigationItem<DashboardViewModel>("Dashboard", "🗂"));
        NavigationItems.Add(CreateNavigationItem<PosViewModel>("Punto de venta", "🛒"));
        NavigationItems.Add(CreateNavigationItem<InventoryViewModel>("Inventario", "📦"));
        NavigationItems.Add(CreateNavigationItem<AppointmentsViewModel>("Consultorio", "🩺"));
        NavigationItems.Add(CreateNavigationItem<PatientsViewModel>("Pacientes", "👥"));
        NavigationItems.Add(CreateNavigationItem<ReportsViewModel>("Reportes", "📊"));
        NavigationItems.Add(CreateNavigationItem<SettingsViewModel>("Configuración", "⚙"));
    }

    public void Initialize(UserSession session)
    {
        if (_sessionService.Current?.UserId != session.UserId)
        {
            _sessionService.SetSession(session);
        }
        else
        {
            UpdateWelcomeMessage(session);
        }
        if (NavigationItems.FirstOrDefault() is NavigationItem defaultItem)
        {
            NavigateTo<DashboardViewModel>(defaultItem);
        }
        else
        {
            NavigateTo<DashboardViewModel>();
        }
    }

    private NavigationItem CreateNavigationItem<TViewModel>(string name, string icon) where TViewModel : class
    {
        var navigationItem = new NavigationItem
        {
            Name = name,
            Icon = icon
        };

        navigationItem.Command = new RelayCommand(() => NavigateTo<TViewModel>(navigationItem));

        return navigationItem;
    }

    private void NavigateTo<TViewModel>(NavigationItem? navigationItem = null) where TViewModel : class
    {
        if (navigationItem is not null)
        {
            SelectedNavigation = navigationItem;
        }

        _navigationService.NavigateTo<TViewModel>();
    }

    private void UpdateWelcomeMessage(UserSession? session)
    {
        WelcomeMessage = session is not null
            ? $"Bienvenido, {session.FullName}"
            : "Bienvenido";
    }
}
