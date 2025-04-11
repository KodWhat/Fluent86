using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Fluent86.Core.VirtualMachines;
using Fluent86.UI.Extensions;

using FluentResults;

using WinRT.Interop;

namespace Fluent86.UI.ViewModels;

public sealed partial class VMListViewModel : ObservableRecipient
{
	private readonly IVirtualMachineManager _virtualMachineManager;

	public ObservableCollection<VirtualMachineInfo> VirtualMachines { get; } = [];

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

	[RelayCommand]
	private void StartVM(VirtualMachineInfo vmInfo)
	{
		System.Diagnostics.Debug.WriteLine($"Start VM {vmInfo.Name}");
		App app = (App.Current as App) ?? throw new ApplicationException("Unable to get current App instance");
		nint winHandle = WindowNative.GetWindowHandle(app.Window);
		_virtualMachineManager.StartVirtualMachine(vmInfo, winHandle);
	}
}
