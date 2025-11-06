using System.IO;
using System.Windows;
using System.Windows.Threading;
using Farmacia.Data.Contexts;
using Farmacia.Data.Seed;
using Farmacia.Data.Services;
using Farmacia.Domain.Services;
using Farmacia.UI.Wpf.Services;
using Farmacia.UI.Wpf.ViewModels;
using Farmacia.UI.Wpf.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using QuestPDF.Infrastructure;

namespace Farmacia.UI.Wpf;

public partial class App : System.Windows.Application
{
	public static IHost AppHost { get; private set; } = null!;
	public static IServiceScope AppScope { get; private set; } = null!;

	public App()
	{
		QuestPDF.Settings.License = LicenseType.Community;
		DispatcherUnhandledException += OnDispatcherUnhandledException;
		AppHost = Host.CreateDefaultBuilder()
	    .UseSerilog((_, logger) =>
			{
				var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
				Directory.CreateDirectory(logsDirectory);
				logger
		    .MinimumLevel.Information()
		    .WriteTo.File(Path.Combine(logsDirectory, "farmacia-.log"), rollingInterval: RollingInterval.Day);
			})
			.ConfigureServices((context, services) =>
			{
				services.AddDbContext<PharmacyDbContext>(options =>
				{
					var dataDir = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "FarmaciaApp");
					Directory.CreateDirectory(dataDir);
					var dbPath = Path.Combine(dataDir, "farmacia.db");
					options.UseSqlite($"Data Source={dbPath}");
				});

				services.AddSingleton<DatabaseSeeder>();
				services.AddScoped<IAuthenticationService, AuthenticationService>();
				services.AddScoped<IInventoryService, InventoryService>();
				services.AddScoped<ISequenceService, SequenceService>();
				services.AddScoped<DatabaseMaintenanceService>();
				services.AddSingleton<NavigationService>();
				services.AddScoped<DashboardService>();
				services.AddScoped<IReportingService, ReportingService>();
				services.AddScoped<IConfigurationService, ConfigurationService>();
				services.AddSingleton<UserSessionService>();

				services.AddTransient<DashboardViewModel>();
				services.AddTransient<LoginViewModel>();
				services.AddTransient<MainViewModel>();
				services.AddTransient<PosViewModel>();
				services.AddTransient<InventoryViewModel>();
				services.AddTransient<AppointmentsViewModel>();
				services.AddTransient<PatientsViewModel>();
				services.AddTransient<ReportsViewModel>();
				services.AddTransient<SettingsViewModel>();

				services.AddTransient<LoginWindow>();
				services.AddTransient<MainWindow>();
			})
			.Build();
	}

	private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		Log.Logger?.Error(e.Exception, "Unhandled exception");
		System.Windows.MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		e.Handled = true;
	}

	protected override async void OnStartup(StartupEventArgs e)
	{
		await AppHost.StartAsync();

		AppScope = AppHost.Services.CreateScope();
		var seeder = AppScope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
		await seeder.SeedAsync();

		var navigationService = AppScope.ServiceProvider.GetRequiredService<NavigationService>();
		navigationService.SetServiceProvider(AppScope.ServiceProvider);

		var loginWindow = AppScope.ServiceProvider.GetRequiredService<LoginWindow>();
		loginWindow.Show();

		base.OnStartup(e);
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		await AppHost.StopAsync();
		AppScope.Dispose();
		AppHost.Dispose();
		base.OnExit(e);
	}
}

