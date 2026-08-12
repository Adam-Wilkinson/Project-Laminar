using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginSource
{
    public string Name { get; }
    
    public IAsyncEnumerable<VersionedPluginId> Reload();
    
    public IReadOnlyList<VersionedPluginId> Plugins { get; }
    
    public Task<IInstalledPlugin> InstallPlugin(
        IPluginInfo plugin, 
        SemanticVersion version, 
        IRuntimeHost runtimeHost,
        CancellationToken cancellationToken = default);
    
    public Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default);
}