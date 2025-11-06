using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Farmacia.UI.Wpf.ViewModels;

namespace Farmacia.UI.Wpf.Views;

public partial class PatientsView : System.Windows.Controls.UserControl
{
    private bool _hasLoaded;

    public PatientsView()
    {
        InitializeComponent();
    }

    private async void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        if (DataContext is PatientsViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            await viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
