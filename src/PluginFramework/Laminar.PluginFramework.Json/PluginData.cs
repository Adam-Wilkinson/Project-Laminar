using Corvus.Json;

namespace Laminar.PluginFramework.Json;

[JsonSchemaTypeGenerator("../plugin.schema.json")]
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