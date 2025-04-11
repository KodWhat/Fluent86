using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.Core.Settings;
using Fluent86.Core.VirtualMachines;
using Fluent86.Core.VirtualMachines.List;
using Fluent86.UI.Localization;
using Fluent86.UI.ViewModels;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

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

		Window = new MainWindow();
		Window.Activate();
	}

	private static void ConfigureServices()
	{
		IServiceCollection services = new ServiceCollection();

		services.AddSingleton<ILocalizationProvider, LocalizationProvider>();

		services.AddSingleton<ISettingsProvider, RegistrySettingsProvider>();
		services.AddSingleton<IVirtualMachineListingProvider, RegistryVirtualMachineListingProvider>();
		services.AddSingleton<IVirtualMachineManager, VirtualMachineManager>();

		// Add ViewModels
		services.AddTransient<VMListViewModel>();

		Ioc.Default.ConfigureServices(services.BuildServiceProvider());
	}
}
