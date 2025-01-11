using Microsoft.Windows.ApplicationModel.Resources;

namespace Fluent86.UI.Localization;

internal class LocalizationProvider : ILocalizationProvider
{
	private readonly ResourceLoader _resourceLoader = new();

	public string GetLocalized(string key) => _resourceLoader.GetString(key);
}
