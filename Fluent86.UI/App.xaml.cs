using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.Core.Settings;
using Fluent86.Core.VirtualMachines;
using Fluent86.Core.VirtualMachines.List;
using Fluent86.UI.Localization;
using Fluent86.UI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using Serilog;

namespace Fluent86.UI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	public Window? Window { get; private set; }

	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		InitializeComponent();

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.Enrich.FromLogContext()
			.WriteTo.Debug()
			.CreateLogger();
	}

	/// <summary>
	/// Invoked when the application is launched.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		ConfigureServices();

		ISettingsProvider settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
		settingsProvider.LoadSettings();

		Window = new MainWindow(Ioc.Default.GetRequiredService<IWindowMessagesListener>());
		Window.Activate();
	}

	private static void ConfigureServices()
	{
		IServiceCollection services = new ServiceCollection();

		services.AddLogging(loggingBuilder => loggingBuilder
			.AddSerilog(dispose: true)
		);

		services.AddSingleton<ILocalizationProvider, LocalizationProvider>();

		services.AddSingleton<ISettingsProvider, RegistrySettingsProvider>();
		services.AddSingleton<IVirtualMachineListingProvider, RegistryVirtualMachineListingProvider>();
		services.AddSingleton<IVirtualMachineManager, VirtualMachineManager>();
		services.AddSingleton<IWindowMessagesListener, WindowMessagesListener>();

		// Add ViewModels
		services.AddTransient<VMListViewModel>();

		Ioc.Default.ConfigureServices(services.BuildServiceProvider());
	}
}
