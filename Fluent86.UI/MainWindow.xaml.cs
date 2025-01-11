using Microsoft.UI.Xaml;

namespace Fluent86.UI;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		// C# code to set AppTitleBar UIElement as Titlebar
		Window window = this;
		window.ExtendsContentIntoTitleBar = true;  // Hides the default system titlebar.
		window.SetTitleBar(TitleBar); // Replace system titlebar with the WinUI Titlebar.
		AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
	}
}
