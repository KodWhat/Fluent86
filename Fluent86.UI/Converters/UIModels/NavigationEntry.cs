using System;

using Microsoft.UI.Xaml.Controls;

namespace Fluent86.UI.Converters.UIModels;

internal sealed record NavigationEntry(NavigationViewItem ViewItem, Type PageType);
