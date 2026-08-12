using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginSourceFactory
{
    public IPluginSource OfflineCache { get; }
    
    public IPluginSource FromPersistentData(IPersistentDictionary persistentDictionary);
}