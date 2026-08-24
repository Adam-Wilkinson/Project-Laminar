using Laminar.Domain;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginSource
{
    public string Name { get; }
    
    public IAsyncEnumerable<VersionedPluginId> Reload();
    
    public IReadOnlyCollection<VersionedPluginId> Plugins { get; }
    
    public bool HasPlugin(VersionedPluginId plugin);
    
    public Task<MayError<IInstalledPlugin>> InstallPlugin(VersionedPluginId pluginId, IRuntimeHost runtimeHost, CancellationToken cancellationToken = default);
    
    public Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default);
}