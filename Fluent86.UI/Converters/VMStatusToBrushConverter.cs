using System;

using Fluent86.Core.VirtualMachines;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Fluent86.UI.Converters;
internal partial class VMStatusToBrushConverter : DependencyObject, IValueConverter
{
	public static readonly DependencyProperty StoppedBrushProperty =
		DependencyProperty.Register(nameof(StoppedBrush), typeof(Brush), typeof(VMStatusToBrushConverter), new PropertyMetadata(null));

	public static readonly DependencyProperty PausedBrushProperty =
		DependencyProperty.Register(nameof(PausedBrush), typeof(Brush), typeof(VMStatusToBrushConverter), new PropertyMetadata(null));

	public static readonly DependencyProperty RunningBrushProperty =
		DependencyProperty.Register(nameof(RunningBrush), typeof(Brush), typeof(VMStatusToBrushConverter), new PropertyMetadata(null));

	public static readonly DependencyProperty WaitingBrushProperty =
		DependencyProperty.Register(nameof(WaitingBrush), typeof(Brush), typeof(VMStatusToBrushConverter), new PropertyMetadata(null));


	public Brush StoppedBrush
	{
		get { return GetValue(StoppedBrushProperty) as Brush ?? new SolidColorBrush(); }
		set { SetValue(StoppedBrushProperty, value); }
	}

	public Brush PausedBrush
	{
		get { return GetValue(PausedBrushProperty) as Brush ?? new SolidColorBrush(); }
		set { SetValue(PausedBrushProperty, value); }
	}

	public Brush RunningBrush
	{
		get { return GetValue(RunningBrushProperty) as Brush ?? new SolidColorBrush(); }
		set { SetValue(RunningBrushProperty, value); }
	}

	public Brush WaitingBrush
	{
		get { return GetValue(WaitingBrushProperty) as Brush ?? new SolidColorBrush(); }
		set { SetValue(WaitingBrushProperty, value); }
	}

	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is not VirtualMachineStatus status)
		{
			return DependencyProperty.UnsetValue;
		}

		return status switch
		{
			VirtualMachineStatus.Stopped => StoppedBrush,
			VirtualMachineStatus.Running => RunningBrush,
			VirtualMachineStatus.Waiting => WaitingBrush,
			VirtualMachineStatus.Paused => PausedBrush,
			_ => DependencyProperty.UnsetValue,
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		return DependencyProperty.UnsetValue;
	}
}
