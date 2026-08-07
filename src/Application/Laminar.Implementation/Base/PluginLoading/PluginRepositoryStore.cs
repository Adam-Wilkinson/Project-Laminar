using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRepositoryStore(IPluginRepositoryFactory factory) : IPluginRepositoryStore
{
    private readonly List<IPluginRepository> _pluginRepositories = [];
    private readonly Dictionary<string, IPluginInfo> _pluginInfos = [];
    
    public IPluginRepository AddFromPersistentDictionary(IPersistentDictionary persistentDictionary)
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

            foreach (var version in pluginInfo.AllVersions)
            {
                masterInfo.AddVersion(version, newRepo);
            }
        }

        return newRepo;
    }
    
    public void ForgetRepository(IPluginRepository repository)
    {
        foreach (var (id, pluginInfo) in repository.Plugins)
        {
            if (!_pluginInfos.TryGetValue(id, out var masterInfo))
            {
                continue;
            }

            foreach (var version in pluginInfo.AllVersions)
            {
                masterInfo.RemoveVersion(version, repository);
                if (masterInfo.AllVersions.Count == 0)
                {
                    _pluginInfos.Remove(id);
                }
            }
        }
    }

    public bool TryGetPluginInfoFromId(string id, [NotNullWhen(true)] out IPluginInfo? pluginInfo)
        => _pluginInfos.TryGetValue(id, out pluginInfo);

    public IReadOnlyList<IPluginRepository> Repositories => _pluginRepositories;
}