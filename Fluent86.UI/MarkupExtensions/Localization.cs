using CommunityToolkit.Mvvm.DependencyInjection;

using Fluent86.UI.Localization;

using Microsoft.UI.Xaml.Markup;

namespace Fluent86.UI.MarkupExtensions;

[MarkupExtensionReturnType(ReturnType = typeof(string))]
internal sealed partial class LocalizationExtension : MarkupExtension
{
	public string Name { get; set; } = string.Empty;

	protected override object ProvideValue()
	{
		ILocalizationProvider localizationProvider = Ioc.Default.GetRequiredService<ILocalizationProvider>();

		return localizationProvider.GetLocalized(Name);
	}
}
