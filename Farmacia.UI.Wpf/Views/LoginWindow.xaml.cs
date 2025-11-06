using System;
using System.Windows;
using System.Windows.Controls;
using Farmacia.UI.Wpf.Models;
using Farmacia.UI.Wpf.Services;
using Farmacia.UI.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacia.UI.Wpf.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly UserSessionService _sessionService;

    public LoginWindow(LoginViewModel viewModel, IServiceProvider serviceProvider, UserSessionService sessionService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        _sessionService = sessionService;
        DataContext = _viewModel;
        _viewModel.LoginSucceeded += OnLoginSucceeded;
    }

    private void OnLoginSucceeded(object? sender, UserSession session)
    {
        Dispatcher.Invoke(() =>
        {
            _sessionService.SetSession(session);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Initialize(session);
            mainWindow.Show();
            _viewModel.LoginSucceeded -= OnLoginSucceeded;
            Close();
        });
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm && sender is PasswordBox passwordBox)
        {
            vm.Password = passwordBox.Password;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _viewModel.LoginSucceeded -= OnLoginSucceeded;
        base.OnClosed(e);
    }
}
