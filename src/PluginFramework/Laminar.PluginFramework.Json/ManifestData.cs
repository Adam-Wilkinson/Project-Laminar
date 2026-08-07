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