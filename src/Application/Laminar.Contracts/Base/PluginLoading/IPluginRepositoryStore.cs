using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepositoryStore
{
    public IReadOnlyList<IPluginRepository> Repositories { get; }
    
    public IPluginRepository AddFromPersistentDictionary(IPersistentDictionary persistentList);

    public void ForgetRepository(IPluginRepository repository);
    
    public bool TryGetPluginInfoFromId(string id, [NotNullWhen(true)] out IPluginInfo? pluginInfo);
}