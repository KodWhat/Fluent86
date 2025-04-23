using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Fluent86.Core.VirtualMachines;
using Fluent86.UI.Extensions;

using Microsoft.Extensions.Logging;

using WinRT.Interop;

namespace Fluent86.UI.ViewModels;

public sealed partial class VMListViewModel : ObservableRecipient
{
	private readonly IVirtualMachineManager _virtualMachineManager;
	private readonly ILogger<VMListViewModel> _logger;

	public ObservableCollection<VirtualMachineInfo> VirtualMachines { get; } = [];

	public VMListViewModel(IVirtualMachineManager virtualMachineManager, ILogger<VMListViewModel> logger)
	{
		_virtualMachineManager = virtualMachineManager;
		_logger = logger;

		LoadVMs();
	}

	private void LoadVMs()
	{
		VirtualMachines.Clear();

		IReadOnlyCollection<VirtualMachineInfo> listVMResult = _virtualMachineManager.VirtualMachines;

		VirtualMachines.AddRange(listVMResult.OrderBy(i => i.Name));
	}

	[RelayCommand]
	private void StartVM(VirtualMachineInfo vmInfo)
	{
		_logger.LogDebug("Start VM {vmName}", vmInfo.Name);
		App app = (App.Current as App) ?? throw new ApplicationException("Unable to get current App instance");
		nint winHandle = WindowNative.GetWindowHandle(app.Window);
		_virtualMachineManager.StartVirtualMachine(vmInfo, winHandle);
	}
}
