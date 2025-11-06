using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.ViewModels;

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

    private void SuggestionItem_OnClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not PosViewModel viewModel)
        {
            return;
        }

        if (sender is not ListBoxItem item)
        {
            return;
        }

        if (item.DataContext is not ProductSuggestionModel suggestion)
        {
            return;
        }

        if (viewModel.SelectSuggestionCommand.CanExecute(suggestion))
        {
            viewModel.SelectSuggestionCommand.Execute(suggestion);
        }

        e.Handled = true;
    }
}
