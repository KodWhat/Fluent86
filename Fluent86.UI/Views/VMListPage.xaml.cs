using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.UI.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI.Views;

public sealed partial class VMListPage : Page
{
	public VMListPage()
	{
		VMListViewModel viewModel = Ioc.Default.GetRequiredService<VMListViewModel>();
		DataContext = viewModel;

		InitializeComponent();
	}
}
