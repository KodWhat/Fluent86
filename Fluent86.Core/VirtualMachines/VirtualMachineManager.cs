using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

using Fluent86.Core.Settings;
using Fluent86.Core.VirtualMachines.List;

using FluentResults;

namespace Fluent86.Core.VirtualMachines;

public class VirtualMachineManager(
		IVirtualMachineListingProvider virtualMachineStorageProvider,
		ISettingsProvider settingsProvider
	) : IVirtualMachineManager
{
	private readonly IVirtualMachineListingProvider _virtualMachineListingProvider = virtualMachineStorageProvider;
	private readonly ISettingsProvider _settingsProvider = settingsProvider;

	public Result<IReadOnlyCollection<VirtualMachineInfo>> ListVirtualMachines()
	{
		return _virtualMachineListingProvider.GetVirtualMachines();
	}

	public Result<VirtualMachineInfo> CreateVirtualMachine(string name, string description, bool createDirectory = true)
	{
		VirtualMachineInfo virtualMachineInfo = new()
		{
			Name = name,
			Description = description,
			Path = _virtualMachineListingProvider.ComputePath(name)
		};

		Result addListingResult = _virtualMachineListingProvider.AddVirtualMachine(virtualMachineInfo);

		if (addListingResult.IsFailed)
		{
			return addListingResult;
		}

		if (createDirectory)
		{
			try
			{
				Directory.CreateDirectory(virtualMachineInfo.Path);
			}
			catch (Exception ex)
			{
				return new ExceptionalError(ex);
			}
		}

		return virtualMachineInfo;
	}

	public Result<VirtualMachineInfo> EditVirtualMachine(VirtualMachineInfo virtualMachineInfo, string newName, string newDescription)
	{
		Result deleteResult = DeleteVirtualMachine(virtualMachineInfo, deleteFiles: false);

		if (deleteResult.IsFailed)
		{
			return Result.Fail($"Error while removing existing entry for vm {virtualMachineInfo.Name}");
		}

		Result<VirtualMachineInfo> createResult = CreateVirtualMachine(newName, newDescription, false);

		if (createResult.IsFailed)
		{
			return Result.Fail($"Error while creating entry for vm {newName}");
		}

		if (virtualMachineInfo.Name == newName)
		{
			return createResult.Value;
		}

		try
		{ //Move the actual VM files too. This will invalidate any paths inside the cfg, but the user is informed to update those manually.
			Directory.Move(virtualMachineInfo.Path, createResult.Value.Path);
			return createResult.Value;
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
	}

	public Result DeleteVirtualMachine(VirtualMachineInfo virtualMachineInfo, bool deleteFiles)
	{
		if (virtualMachineInfo.Status is not VirtualMachineStatus.Stopped)
		{
			return Result.Fail("VM is not stopped");
		}

		Result removeResult = _virtualMachineListingProvider.RemoveVirtualMachine(virtualMachineInfo);

		if (removeResult.IsFailed)
		{
			return Result.Fail("Failed to remove vm from listing");
		}

		if (deleteFiles is false)
		{
			return Result.Ok();
		}

		try
		{
			Directory.Delete(virtualMachineInfo.Path, true);

			return Result.Ok();
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
	}

	public Result<VirtualMachineInfo> ImportVirtualMachine(string sourcePath, string name, string description)
	{
		VirtualMachineInfo virtualMachineInfo = new()
		{
			Name = name,
			Description = description,
			Path = _virtualMachineListingProvider.ComputePath(name)
		};

		Result addListingResult = _virtualMachineListingProvider.AddVirtualMachine(virtualMachineInfo);

		if (addListingResult.IsFailed)
		{
			return addListingResult;
		}

		//Copy existing files to the new VM directory
		try
		{
			Directory.CreateDirectory(virtualMachineInfo.Path);

			foreach (string oldPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(oldPath.Replace(sourcePath, virtualMachineInfo.Path));
			}
			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, virtualMachineInfo.Path), true);
			}
		}
		catch (Exception ex)
		{
			// Revert ?
			return new ExceptionalError(ex);
		}

		return virtualMachineInfo;
	}

	public Result StartVirtualMachine(VirtualMachineInfo virtualMachineInfo, nint f86Handle)
	{
		string handleHexString = string.Format("{0:X16}", f86Handle);
		string uidHexString = string.Format("{0:X16}", unchecked((uint)virtualMachineInfo.Path.GetHashCode()));

		Process p = new Process();
		p.StartInfo.FileName = _settingsProvider.SettingsValues.BoxExePath;
		string arguments = $"--vmpath \"{virtualMachineInfo.Path}\"";
		arguments += $" --hwnd {uidHexString},{handleHexString}";

		if (_settingsProvider.SettingsValues.LoggingEnabled)
		{
			arguments += $" --logfile \"{_settingsProvider.SettingsValues.LogPath}\"";
		}

		if (_settingsProvider.SettingsValues.ShowConsole)
		{
			p.StartInfo.RedirectStandardOutput = true;
			p.StartInfo.UseShellExecute = false;
		}

		p.StartInfo.Arguments = arguments;

		bool success = p.Start();

		virtualMachineInfo.RunningProcessId = p.Id;
		virtualMachineInfo.Status = VirtualMachineStatus.Running;

		return Result.Ok();
	}

	public Result StopVirtualMachine()
	{
		throw new System.NotImplementedException();
	}

	public Result ForceStopVirtualMachine()
	{
		throw new System.NotImplementedException();
	}

	public Result PauseVirtualMachine()
	{
		throw new System.NotImplementedException();
	}

	public Result ResumeVirtualMachine()
	{
		throw new System.NotImplementedException();
	}

	public Result<bool> IsNameInUse(string name)
	{
		return _virtualMachineListingProvider.IsNameInUse(name);
	}

	public Result ClearCmos(VirtualMachineInfo virtualMachineInfo)
	{
		try
		{
			Directory.Delete(Path.Combine(virtualMachineInfo.Path, "nvr"), true);
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return new ExceptionalError(ex);
		}
	}
}
