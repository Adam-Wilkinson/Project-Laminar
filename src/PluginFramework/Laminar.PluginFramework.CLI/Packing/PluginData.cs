using Corvus.Json;

namespace Laminar.PluginFramework.CLI.Packing;

[JsonSchemaTypeGenerator("../../plugin.schema.json")]
public readonly partial struct PluginData
{
    public string GetPluginVersion()
    {
        var pluginFullName = $"{(int)MajorVersion}.{(int)MinorVersion}";
        
        if (PatchVersion.IsJsonNumber)
        {
            pluginFullName += $".{(int)PatchVersion}";
        }

        if (PrereleaseVersion.AsString.HasJsonElementBacking)
        {
            pluginFullName += "-" + (string)PrereleaseVersion;
        }

        return pluginFullName;
    }
}

[JsonSchemaTypeGenerator("../../manifest.schema.json")]
public readonly partial struct ManifestData;

public static class ManifestDataExtensions
{
    extension(ManifestData)
    {
        public static ManifestData FromPluginData(PluginData data)
        {
            return ManifestData.Create(
                ManifestData.IdEntity.FromJson(data.Id.AsJsonElement),
                data.GetPluginVersion(),
                description: ManifestData.LangSupportedString.FromJson(data.Description.AsJsonElement), 
                pluginDependencies: ManifestData.PluginDependenciesArray.FromJson(data.PluginDependencies.AsJsonElement), 
                userFriendlyName: ManifestData.LangSupportedString.FromJson(data.UserFriendlyName.AsJsonElement),
                supportedPlatforms: ManifestData.SupportedPlatformsArray.FromJson(data.SupportedPlatforms.AsJsonElement)
            );
        }
    }
}