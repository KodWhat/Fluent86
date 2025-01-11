using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;

using Fluent86.Core.VirtualMachines;
using Fluent86.UI.Extensions;

using FluentResults;

namespace Fluent86.UI.ViewModels;

public sealed partial class VMListViewModel : ObservableRecipient
{
	private readonly IVirtualMachineManager _virtualMachineManager;

	public VMListViewModel(IVirtualMachineManager virtualMachineManager)
	{
		_virtualMachineManager = virtualMachineManager;
		LoadVMs();
	}

	private void LoadVMs()
	{
		VirtualMachines.Clear();

		Result<IReadOnlyCollection<VirtualMachineInfo>> listVMResult = _virtualMachineManager.ListVirtualMachines();

		if (listVMResult.IsFailed)
		{
			return;
		}

		VirtualMachines.AddRange(listVMResult.Value.OrderBy(i => i.Name));
	}

	public ObservableCollection<VirtualMachineInfo> VirtualMachines { get; } = [];
}
