using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Fluent86.Core.VirtualMachines;

public partial class VirtualMachineInfo : ObservableObject
{
	public required string Name { get; set; }

	public string Description { get; set; } = string.Empty;

	public required string Path { get; set; }

	[ObservableProperty]
	public VirtualMachineStatus _status = VirtualMachineStatus.Stopped;

	public int RunningProcessId { get; set; } = 0;

	public IntPtr RunningWindowHandle { get; set; } = IntPtr.Zero;
}
