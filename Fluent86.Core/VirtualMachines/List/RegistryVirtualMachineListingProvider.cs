using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Fluent86.Core.Classic;
using Fluent86.Core.Settings;

using FluentResults;

using Microsoft.Win32;

namespace Fluent86.Core.VirtualMachines.List;

public class RegistryVirtualMachineListingProvider(ISettingsProvider settingsProvider) : IVirtualMachineListingProvider
{
	private const string VmKey = @"SOFTWARE\86Box\Virtual Machines";

	private readonly ISettingsProvider _settingsProvider = settingsProvider;

	public Result<IReadOnlyCollection<VirtualMachineInfo>> GetVirtualMachines()
	{
		// Disable warning for obsolete method as it is needed for compatibility purposes.
#pragma warning disable SYSLIB0011 // Type or member is obsolete
		try
		{
			RegistryKey? regKey = Registry.CurrentUser.CreateSubKey(VmKey);

			if (regKey == null)
			{
				return Result.Fail($"Can't open or create registry key {VmKey}");
			}

			List<VirtualMachineInfo> virtualMachineInfo = [];

			foreach (string value in regKey.GetValueNames())
			{
				using MemoryStream ms = new MemoryStream((byte[])regKey.GetValue(value)!);

				SerializationBinder binder = new ClassicSerializationBinder();

				BinaryFormatter bf = new BinaryFormatter
				{
					Binder = binder
				};
				VM vm = (VM)bf.Deserialize(ms);

				virtualMachineInfo.Add(new VirtualMachineInfo
				{
					Name = vm.Name,
					Description = vm.Desc,
					Path = Path.Combine(_settingsProvider.SettingsValues.VmPath, vm.Name),
					Status = VirtualMachineStatus.Stopped
				});
			}

			regKey.Close();

			return virtualMachineInfo;
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
#pragma warning restore SYSLIB0011 // Type or member is obsolete
	}

	public Result AddVirtualMachine(VirtualMachineInfo virtualMachineInfo)
	{
		// Disable warning for obsolete method as it is needed for compatibility purposes.
#pragma warning disable SYSLIB0011 // Type or member is obsolete
		using MemoryStream ms = new MemoryStream();

		SerializationBinder binder = new ClassicSerializationBinder();

		BinaryFormatter bf = new BinaryFormatter
		{
			Binder = binder
		};

		// Create classic VM object
		VM classicVM = new VM(virtualMachineInfo.Name, virtualMachineInfo.Description, virtualMachineInfo.Path);

		try
		{
			bf.Serialize(ms, classicVM);
			byte[] data = ms.ToArray();

			RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(VmKey, writable: true);

			if (registryKey is null)
			{
				return Result.Fail($"Can't open or create registry key {VmKey}");
			}

			registryKey.SetValue(virtualMachineInfo.Name, data, RegistryValueKind.Binary);
			registryKey.Close();

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
#pragma warning restore SYSLIB0011 // Type or member is obsolete
	}

	public string ComputePath(string vmName)
	{
		return Path.Combine(_settingsProvider.SettingsValues.VmPath, vmName);
	}

	public Result RemoveVirtualMachine(VirtualMachineInfo virtualMachineInfo)
	{
		try
		{
			RegistryKey? regkey = Registry.CurrentUser.OpenSubKey(VmKey, true);

			if (regkey == null)
			{
				return Result.Fail($"Can't open or create registry key {VmKey}");
			}

			regkey.DeleteValue(virtualMachineInfo.Name);
			regkey.Close();

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
	}

	public Result<bool> IsNameInUse(string name)
	{
		try
		{
			RegistryKey? regkey = Registry.CurrentUser.OpenSubKey(VmKey, true);

			if (regkey == null)
			{
				// In this case, if we can't find the VM subkey, it means no VM exists
				return false;
			}

			if (regkey.GetValue(name) is not null)
			{
				regkey.Close();
				return true;
			}

			regkey.Close();
			return false;
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
	}
}
