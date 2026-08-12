using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginLibrary
{
    public IReadOnlyList<IPluginSource> Sources { get; }
    
    public IReadOnlyObservableCollection<IPluginSource> CurrentlyLoadingSources { get; }
    
    public Task EnsurePluginsLoaded();
    
    public IReadOnlyObservableCollection<IPluginInfo> LoadedPlugins { get; }
    
    public Task AddSource(IPluginSource source);
    
    public void ForgetSource(IPluginSource source);
    
    public Task<IPluginInfo?> GetPluginInfoOrNull(VersionedPluginId pluginId);
}