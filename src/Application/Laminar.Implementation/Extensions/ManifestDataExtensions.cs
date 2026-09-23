using System.Text.Json;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Extensions;

public static class ManifestDataExtensions
{
    extension(ManifestData data)
    {
        public VersionedPluginId GetId() => new((string)data.Id, new SemanticVersion((string)data.Version));

        public IEnumerable<(VersionedPluginId, FrontendDependency)> GetDependencies()
        {
            if (data.PluginDependencies.ValueKind is not JsonValueKind.Array)
            {
                yield break;
            }

            foreach (var dependency in data.PluginDependencies)
            {                
                yield return (new VersionedPluginId((string)dependency.PluginName,
                    new SemanticVersion((string)dependency.PluginVersion)), GetFrontendDependency(dependency.FrontendCondition));
            }
        }
    }

    private static FrontendDependency GetFrontendDependency(ManifestData.Frontend frontend)
    {
        if (Equals(frontend, ManifestData.Frontend.EnumValues.Avalonia))
        {
            return FrontendDependency.Avalonia;
        }
        
        return FrontendDependency.None;
    }
}