using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepositoryStore
{
    public IReadOnlyList<IPluginRepository> Repositories { get; }
    
    public IReadOnlyObservableCollection<IPluginRepository> CurrentlyLoadingRepositories { get; }
    
    public Task EnsurePluginsLoaded();
    
    public IReadOnlyObservableCollection<IPluginInfo> LoadedPlugins { get; }
    
    public Task<IPluginRepository?> AddFromPersistentDictionary(IPersistentDictionary persistentList);
    
    public void ForgetRepository(IPluginRepository repository);
    
    public Task<IPluginInfo?> GetPluginInfoOrNull(VersionedPluginId pluginId);
}