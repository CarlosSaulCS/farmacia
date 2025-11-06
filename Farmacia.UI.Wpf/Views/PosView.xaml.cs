using System.Windows;
using System.Windows.Controls;

namespace Farmacia.UI.Wpf.Views;

public partial class PosView : System.Windows.Controls.UserControl
{
    public PosView()
    {
        InitializeComponent();
    }

    private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        SearchTextBox.Focus();
    }
}
