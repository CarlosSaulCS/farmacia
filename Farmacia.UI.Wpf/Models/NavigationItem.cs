using CommunityToolkit.Mvvm.Input;

namespace Farmacia.UI.Wpf.Models;

public class NavigationItem
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public IRelayCommand Command { get; set; } = null!;
}
