using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.Core.VirtualMachines;
using Fluent86.UI.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI.Views;

public sealed partial class VMListPage : Page
{
	private readonly VMListViewModel _viewModel;
	public VMListPage()
	{
		_viewModel = Ioc.Default.GetRequiredService<VMListViewModel>();
		DataContext = _viewModel;

		InitializeComponent();
	}

	private void VMListItem_StartClicked(VirtualMachineInfo virtualMachine)
	{
		_viewModel.StartVMCommand.Execute(virtualMachine);
	}
}
