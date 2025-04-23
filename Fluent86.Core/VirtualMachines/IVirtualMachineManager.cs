using System.Collections.Generic;

using FluentResults;

namespace Fluent86.Core.VirtualMachines;

public interface IVirtualMachineManager
{
	IReadOnlyCollection<VirtualMachineInfo> VirtualMachines { get; }

	Result<VirtualMachineInfo> CreateVirtualMachine(string name, string description, bool createDirectory = true);

	Result<VirtualMachineInfo> EditVirtualMachine(VirtualMachineInfo virtualMachineInfo, string newName, string newDescription);

	Result DeleteVirtualMachine(VirtualMachineInfo virtualMachineInfo, bool deleteFiles);

	Result<VirtualMachineInfo> ImportVirtualMachine(string sourcePath, string name, string description);

	Result StartVirtualMachine(VirtualMachineInfo virtualMachineInfo, nint f86Handle);

	Result StopVirtualMachine();

	Result ForceStopVirtualMachine();

	Result PauseVirtualMachine();

	Result ResumeVirtualMachine();

	Result UpdateStatus(nint runningHandle, VirtualMachineStatus newStatus);

	Result Set86BoxHandleToVmByUid(nint uid, nint runningWindowHandle);

	Result ClearCmos(VirtualMachineInfo virtualMachineInfo);

	Result<bool> IsNameInUse(string name);
}
