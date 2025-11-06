using System.Windows;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.ViewModels;

namespace Farmacia.UI.Wpf.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void Initialize(UserSession session)
    {
        _viewModel.Initialize(session);
    }
}