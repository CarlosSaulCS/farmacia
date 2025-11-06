using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Domain.Services;
using Farmacia.UI.Wpf.Models;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;

    public event EventHandler<UserSession>? LoginSucceeded;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    public LoginViewModel(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var user = await _authenticationService.LoginAsync(Username, Password);
            if (user is null)
            {
                ErrorMessage = "Credenciales inválidas.";
                return;
            }

            LoginSucceeded?.Invoke(this, new UserSession(user.Id, user.FullName, user.Role.ToString()));
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
