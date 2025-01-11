using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.UI.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI.Views;

public sealed partial class VMListView : Page
{
	public VMListView()
	{
		VMListViewModel viewModel = Ioc.Default.GetRequiredService<VMListViewModel>();
		DataContext = viewModel;

		InitializeComponent();
	}
}
