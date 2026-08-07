using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepository
{
    public string Id { get; }
    
    public void Refresh();
    
    public Dictionary<string, PluginInfo> Plugins { get; }
}