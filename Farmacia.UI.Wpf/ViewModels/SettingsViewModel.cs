using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacia.Data.Services;
using Farmacia.Domain.Services;
using WinForms = System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Farmacia.UI.Wpf.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
	private readonly DatabaseMaintenanceService _maintenanceService;
	private readonly IConfigurationService _configurationService;
	private bool _isApplyingSettings;

	[ObservableProperty]
	private bool _isBusy;

	[ObservableProperty]
	private string? _statusMessage;

	[ObservableProperty]
	private string? _lastBackupPath;

	[ObservableProperty]
	private string _storeName = string.Empty;

	[ObservableProperty]
	private string _storeAddress = string.Empty;

	[ObservableProperty]
	private string _storePhone = string.Empty;

	[ObservableProperty]
	private string _ticketFooter = string.Empty;

	[ObservableProperty]
	private bool _hasUnsavedChanges;

	public SettingsViewModel(DatabaseMaintenanceService maintenanceService, IConfigurationService configurationService)
	{
		_maintenanceService = maintenanceService;
		_configurationService = configurationService;

		_ = LoadSettingsAsync();
	}

	public bool CanExecuteMaintenance => !IsBusy;

	private bool CanReloadSettings() => !IsBusy;

	private bool CanPersistSettings() => !IsBusy && HasUnsavedChanges && !string.IsNullOrWhiteSpace(StoreName);

	partial void OnIsBusyChanged(bool value)
	{
		CreateBackupCommand.NotifyCanExecuteChanged();
		RestoreBackupCommand.NotifyCanExecuteChanged();
		LoadSettingsCommand.NotifyCanExecuteChanged();
		SaveSettingsCommand.NotifyCanExecuteChanged();
	}

	partial void OnHasUnsavedChangesChanged(bool value)
	{
		SaveSettingsCommand.NotifyCanExecuteChanged();
	}

	partial void OnStoreNameChanged(string value) => MarkAsDirty();

	partial void OnStoreAddressChanged(string value) => MarkAsDirty();

	partial void OnStorePhoneChanged(string value) => MarkAsDirty();

	partial void OnTicketFooterChanged(string value) => MarkAsDirty();

	private void MarkAsDirty()
	{
		if (_isApplyingSettings)
		{
			return;
		}

		HasUnsavedChanges = true;
		SaveSettingsCommand.NotifyCanExecuteChanged();
	}

	[RelayCommand(CanExecute = nameof(CanExecuteMaintenance))]
	private async Task CreateBackupAsync()
	{
		using var dialog = new WinForms.FolderBrowserDialog
		{
			Description = "Selecciona la carpeta donde se guardará el respaldo"
		};

		if (dialog.ShowDialog() != WinForms.DialogResult.OK)
		{
			return;
		}

		try
		{
			IsBusy = true;
			var path = await _maintenanceService.CreateBackupAsync(dialog.SelectedPath);
			await _configurationService.SetLastBackupPathAsync(path);
			LastBackupPath = path;
			StatusMessage = $"Respaldo creado: {path}";
		}
		catch (Exception ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand(CanExecute = nameof(CanExecuteMaintenance))]
	private async Task RestoreBackupAsync()
	{
		var dialog = new OpenFileDialog
		{
			Filter = "Base de datos SQLite (*.db)|*.db",
			Title = "Selecciona un respaldo"
		};

		if (dialog.ShowDialog() != true)
		{
			return;
		}

		try
		{
			IsBusy = true;
			await _maintenanceService.RestoreBackupAsync(dialog.FileName);
			await _configurationService.SetLastBackupPathAsync(dialog.FileName);
			LastBackupPath = dialog.FileName;
			StatusMessage = "Base de datos restaurada correctamente.";
		}
		catch (Exception ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand(CanExecute = nameof(CanReloadSettings))]
	private async Task LoadSettingsAsync()
	{
		try
		{
			IsBusy = true;
			_isApplyingSettings = true;

			var settings = await _configurationService.GetGeneralSettingsAsync();
			LastBackupPath = await _configurationService.GetLastBackupPathAsync();

			StoreName = settings.StoreName ?? string.Empty;
			StoreAddress = settings.StoreAddress ?? string.Empty;
			StorePhone = settings.PhoneNumber ?? string.Empty;
			TicketFooter = settings.TicketFooter ?? string.Empty;

			HasUnsavedChanges = false;
			StatusMessage = "Configuración cargada.";
		}
		catch (Exception ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			_isApplyingSettings = false;
			IsBusy = false;
			SaveSettingsCommand.NotifyCanExecuteChanged();
		}
	}

	[RelayCommand(CanExecute = nameof(CanPersistSettings))]
	private async Task SaveSettingsAsync()
	{
		if (string.IsNullOrWhiteSpace(StoreName))
		{
			StatusMessage = "El nombre de la tienda es obligatorio.";
			return;
		}

		try
		{
			IsBusy = true;

			var settings = new GeneralSettings(
				StoreName.Trim(),
				StoreAddress.Trim(),
				StorePhone.Trim(),
				TicketFooter.Trim());

			await _configurationService.SaveGeneralSettingsAsync(settings);

			HasUnsavedChanges = false;
			StatusMessage = "Configuración guardada.";
		}
		catch (Exception ex)
		{
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
	}
}
