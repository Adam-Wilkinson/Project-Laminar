using System.Globalization;
using System.Text.Json;
using Corvus.Json;

namespace Laminar.PluginFramework.Json;

[JsonSchemaTypeGenerator("../manifest.schema.json")]
public readonly partial struct ManifestData
{
    public static ManifestData FromPluginData(PluginData data) =>
        Create(
            IdEntity.FromJson(data.Id.AsJsonElement),
            data.GetPluginVersion(),
            description: LangSupportedString.FromJson(data.Description.AsJsonElement), 
            pluginDependencies: PluginDependenciesArray.FromJson(data.PluginDependencies.AsJsonElement), 
            userFriendlyName: LangSupportedString.FromJson(data.UserFriendlyName.AsJsonElement),
            supportedPlatforms: SupportedPlatformsArray.FromJson(data.SupportedPlatforms.AsJsonElement)
        );
}

public static class LangSupportedStringExtensions
{
    public static string? Localize(this ManifestData.LangSupportedString langSupportedString, CultureInfo cultureInfo)
    {
        if (langSupportedString.ValueKind == JsonValueKind.Undefined)
        {
            return null;
        }
        
        if (langSupportedString.TryGetProperty(cultureInfo.TwoLetterISOLanguageName, out var lang)
            && lang.ValueKind == JsonValueKind.String)
        {
            return (string)lang.AsString;
        }
        
        if (langSupportedString.TryGetString(out var str))
        {
            return str;
        }

        return null;
    }
}