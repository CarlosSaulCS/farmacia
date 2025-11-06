using Microsoft.Extensions.DependencyInjection;

namespace Farmacia.UI.Wpf.Services;

public class NavigationService
{
    private IServiceProvider? _serviceProvider;

    public event EventHandler<object?>? CurrentViewChanged;

    public object? CurrentView { get; private set; }

    public void SetServiceProvider(IServiceProvider provider)
    {
        _serviceProvider = provider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider has not been initialized.");
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentView = viewModel;
        CurrentViewChanged?.Invoke(this, viewModel);
    }
}
