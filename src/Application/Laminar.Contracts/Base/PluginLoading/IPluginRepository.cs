using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Json;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepository
{
    public string Id { get; }
    
    public IAsyncEnumerable<IPluginInfo> Reload();
    
    public Dictionary<string, IPluginInfo> Plugins { get; }
    
    public Task<Stream> StreamPlugin(string id, SemanticVersion version, CancellationToken cancellationToken = default);

    public Task<ManifestData> GetManifest(string id, SemanticVersion version, CancellationToken cancellationToken = default);
}