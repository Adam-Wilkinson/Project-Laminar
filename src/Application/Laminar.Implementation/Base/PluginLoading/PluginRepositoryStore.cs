using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRepositoryStore(IPluginRepositoryFactory factory) : IPluginRepositoryStore
{
    private readonly List<IPluginRepository> _pluginRepositories = [];
    private readonly Dictionary<string, PluginInfo> _pluginInfos = [];
    
    public void AddFromPersistentList(IPersistentList persistentList)
    {
        foreach (var dataPoint in persistentList)
        {
            AddSingleFromPersistentDictionary(dataPoint.GetOrCreateCollection<IPersistentDictionary>());
        }
    }

    public PluginInfo TryGetPluginInfoFromId(string id)
    {
        throw new NotImplementedException();
    }

    public bool TryFindVersionedPlugin(string id, SemanticVersion version, [NotNullWhen(true)] out VersionedPluginInfo? pluginInfo)
    {
        throw new NotImplementedException();
    }

    private void AddSingleFromPersistentDictionary(IPersistentDictionary persistentDictionary)
    {
        var newRepo = factory.FromPersistentData(persistentDictionary);
        _pluginRepositories.Add(factory.FromPersistentData(persistentDictionary));
        foreach (var (id, pluginInfo) in newRepo.Plugins)
        {
            if (!_pluginInfos.TryGetValue(id, out var masterInfo))
            {
                masterInfo = new PluginInfo(id, []);
                _pluginInfos.Add(id, masterInfo);
            }
            
            masterInfo.MergeFrom(pluginInfo);
        }
    }
}