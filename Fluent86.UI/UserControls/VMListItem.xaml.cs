using Fluent86.Core.VirtualMachines;

using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI.UserControls;

public sealed partial class VMListItem : UserControl
{
	public event VMListItemEventHandler? StartClicked;

	public VMListItem()
	{
		InitializeComponent();
	}

	private void StartVM_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		if (DataContext is not VirtualMachineInfo virtualMachineInfo)
		{
			return;
		}

		StartClicked?.Invoke(virtualMachineInfo);
	}
}

public delegate void VMListItemEventHandler(VirtualMachineInfo virtualMachine);