using Microsoft.UI.Xaml;

using WinRT.Interop;

namespace Fluent86.UI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
	private readonly IWindowMessagesListener _windowMessagesListener;

	public MainWindow(IWindowMessagesListener windowMessagesListener)
	{
		InitializeComponent();
		Shell.SetTitleBar(this);
		_windowMessagesListener = windowMessagesListener;

		_windowMessagesListener.AttachListenerToHandle(WindowNative.GetWindowHandle(this));
	}
}
