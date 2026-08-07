using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepository
{
    public string Id { get; }
    
    public void Refresh();
    
    public Dictionary<string, IPluginInfo> Plugins { get; }
    
    public Task<Stream> StreamPlugin(string id, SemanticVersion version, CancellationToken cancellationToken = default);
}