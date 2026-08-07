using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginRepositoryStore
{
    public void AddFromPersistentList(IPersistentList persistentList);

    public PluginInfo TryGetPluginInfoFromId(string id);
    
    public bool TryFindVersionedPlugin(string id, SemanticVersion version, [NotNullWhen(true)] out VersionedPluginInfo? pluginInfo);
}