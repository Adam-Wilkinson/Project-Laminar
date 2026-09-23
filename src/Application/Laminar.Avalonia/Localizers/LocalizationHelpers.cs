using Laminar.Domain.ValueObjects;
using Laminar.Avalonia.Translations;

namespace Laminar.Avalonia.Localizers;

public static class LocalizationHelpers
{
    extension(VersionedPluginId pluginId)
    {
        public string Localize() => string.Format(Strings.PluginVersion.CurrentValue, pluginId.Name, pluginId.Version);
    }
}