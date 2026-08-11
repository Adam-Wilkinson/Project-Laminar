using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepository
{
    public string Id { get; }
    
    public IAsyncEnumerable<VersionedPluginId> Reload();
    
    public IReadOnlyList<VersionedPluginId> Plugins { get; }
    
    public Task<Stream> StreamPlugin(VersionedPluginId plugin, CancellationToken cancellationToken = default);

    public Task<ManifestData> GetManifest(VersionedPluginId plugin, CancellationToken cancellationToken = default);
}