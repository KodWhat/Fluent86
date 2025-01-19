using System;
using System.Collections.Generic;
using System.Linq;

using Fluent86.UI.Converters.UIModels;
using Fluent86.UI.Views;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI;

public sealed partial class Shell : UserControl
{
	private readonly IReadOnlyCollection<NavigationEntry> _navigationEntries;

	public Shell()
	{
		InitializeComponent();

		_navigationEntries =
		[
			new NavigationEntry(MachinesNavItem, typeof(VMListPage)),
			new NavigationEntry(SettingsNavItem, typeof(SettingsPage))
		];
	}

	public void SetTitleBar(Window window)
	{
		window.ExtendsContentIntoTitleBar = true;  // Hides the default system titlebar.
		window.SetTitleBar(TitleBar); // Replace system titlebar with the WinUI Titlebar.
		window.AppWindow.TitleBar.PreferredHeightOption = Microsoft.UI.Windowing.TitleBarHeightOption.Tall;
	}

	private void Shell_OnLoaded(object sender, RoutedEventArgs e)
	{
		NavView.SelectedItem = MachinesNavItem;
		NavFrame.Navigate(typeof(VMListPage));
	}

	private void NavView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
	{
		if (_navigationEntries.FirstOrDefault(i => i.ViewItem == args.InvokedItemContainer)?.PageType is Type pageType)
		{
			NavFrame.Navigate(pageType);
		}
	}
}
