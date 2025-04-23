using System;
using System.Runtime.InteropServices;

using Fluent86.Core.VirtualMachines;

using FluentResults;

using Microsoft.Extensions.Logging;

namespace Fluent86.UI;

internal partial class WindowMessagesListener(
	ILogger<WindowMessagesListener> logger,
	IVirtualMachineManager virtualMachineManager
) : IWindowMessagesListener
{
	private delegate int SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);

	[LibraryImport("Comctl32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);

	[LibraryImport("Comctl32.dll", SetLastError = true)]
	private static partial int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

	private SUBCLASSPROC? _subClassDelegate;

	private const int MSG_86BOX_MAINWIN_INITIALIZED = 0x8891;
	private const int MSG_86BOX_VM_PAUSED_OR_RESUMED = 0x8895;
	private const int MSG_86BOX_DIALOG_OPENED = 0x8896;
	private const int MSG_86BOX_SHUTDOWN_CONFIRMED = 0x8897;

	private const int VM_RESUMED = 0;
	private const int VM_PAUSED = 1;

	private readonly ILogger<WindowMessagesListener> _logger = logger;
	private readonly IVirtualMachineManager _virtualMachineManager = virtualMachineManager;

	public Result AttachListenerToHandle(IntPtr hWnd)
	{
		_logger.LogDebug("Attaching to handle {hWnd:X}", hWnd);
		if (_subClassDelegate != null)
		{
			_logger.LogWarning($"{nameof(AttachListenerToHandle)} was already called once.");
		}

		_subClassDelegate = new SUBCLASSPROC(MessageHandler);

		bool setWindowSubclassResult = SetWindowSubclass(hWnd, _subClassDelegate, 0, 0);

		return setWindowSubclassResult ? Result.Ok() : Result.Fail("SetWindowSubclass call failed");
	}

	private int MessageHandler(nint hWnd, uint message, nint wParam, nint lParam, nint uIdSubclass, uint dwRefData)
	{
		switch (message)
		{
			case MSG_86BOX_MAINWIN_INITIALIZED:
				Process86BoxInitializedMessage(wParam, lParam);
				break;

			case MSG_86BOX_VM_PAUSED_OR_RESUMED:
				Process86BoxPausedOrResumedMessage(wParam, lParam);
				break;

			case MSG_86BOX_SHUTDOWN_CONFIRMED:
				Process86BoxShutdownMessage(lParam);
				break;
		}

		return DefSubclassProc(hWnd, message, wParam, lParam);
	}

	private void Process86BoxInitializedMessage(nint uid, nint runningWindowHandle)
	{
		if (runningWindowHandle == IntPtr.Zero)
		{
			return;
		}

		Result result = _virtualMachineManager.Set86BoxHandleToVmByUid(uid, runningWindowHandle);
		if (result.IsFailed)
		{
			_logger.LogError("Processing 86Box initialized message with running window handle {runningWindowHandle} failed with error {error}", runningWindowHandle, result.Reasons);
		}

		_virtualMachineManager.UpdateStatus(runningWindowHandle, VirtualMachineStatus.Running);
	}

	private void Process86BoxPausedOrResumedMessage(nint newState, nint runningWindowHandle)
	{
		Result result;
		switch (newState)
		{
			case VM_PAUSED:
				result = _virtualMachineManager.UpdateStatus(runningWindowHandle, VirtualMachineStatus.Paused);
				break;
			case VM_RESUMED:
				result = _virtualMachineManager.UpdateStatus(runningWindowHandle, VirtualMachineStatus.Running);
				break;
			default:
				return;
		}

		if (result.IsFailed)
		{
			_logger.LogError("Processing 86Box paused/resumed message with running window handle {runningWindowHandle} failed with error {error}", runningWindowHandle, result.Reasons);
		}
	}

	private void Process86BoxShutdownMessage(nint runningWindowHandle)
	{
		Result result = _virtualMachineManager.UpdateStatus(runningWindowHandle, VirtualMachineStatus.Stopped);

		if (result.IsFailed)
		{
			_logger.LogError("Processing 86Box shutdown message with running window handle {runningWindowHandle} failed with error {error}", runningWindowHandle, result.Reasons);
		}
	}
}
