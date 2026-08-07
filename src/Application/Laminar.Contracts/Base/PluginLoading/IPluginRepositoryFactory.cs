using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepositoryFactory
{
    public IPluginRepository FromPersistentData(IPersistentDictionary persistentDictionary);
}