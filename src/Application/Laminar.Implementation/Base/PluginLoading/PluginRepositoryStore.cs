using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRepositoryStore(IPluginRepositoryFactory factory, IExceptionHandler exceptionHandler) : IPluginRepositoryStore
{
    private readonly List<IPluginRepository> _pluginRepositories = [];
    private readonly Dictionary<string, IPluginInfo> _pluginInfos = [];
    private readonly Dictionary<(string id, SemanticVersion version), TaskCompletionSource<IPluginInfo?>> _pendingRequests = [];
    private readonly ObservableCollection<IPluginInfo> _loadedPlugins = [];
    private readonly ObservableCollection<IPluginRepository> _loadingRepositories = [];
    private readonly Lock _loadingRepositoriesLock = new();
    private readonly Lock _pluginInfosLock = new();
    
    private TaskCompletionSource? _loadedCompletionSource;
    
    public async Task<IPluginRepository> AddFromPersistentDictionary(IPersistentDictionary persistentDictionary)
    {
        var newRepo = factory.FromPersistentData(persistentDictionary);
        _pluginRepositories.Add(factory.FromPersistentData(persistentDictionary));
        
        lock (_loadingRepositoriesLock)
        {
            if (_loadingRepositories.Count == 0)
            {
                _loadedCompletionSource = new TaskCompletionSource();
            }
            _loadingRepositories.Add(newRepo);   
        }

        try
        {
            await foreach (var pluginInfo in newRepo.Reload())
            {
                MergePluginInfo(pluginInfo, newRepo);
            }
        }
        catch (Exception ex)
        {
            await exceptionHandler.OnExceptionAsync(ex);
        }
        finally
        {
            lock (_loadingRepositoriesLock)
            {
                _loadingRepositories.Remove(newRepo);
                if (_loadingRepositories.Count == 0)
                {
                    _loadedCompletionSource?.SetResult();
                    _loadedCompletionSource = null;
                }  
            } 
        }

        
        return newRepo;
    }

    public Task<IPluginInfo?> GetPluginInfoOrNull(string pluginId, SemanticVersion version)
    {
        if (_pluginInfos.TryGetValue(pluginId, out var pluginInfo) && pluginInfo.HasVersion(version))
        {
            return Task.FromResult<IPluginInfo?>(pluginInfo);
        }

        if (_pendingRequests.TryGetValue((pluginId, version), out var pendingRequest))
        {
            return pendingRequest.Task;
        }
        
        pendingRequest = new TaskCompletionSource<IPluginInfo?>();
        _pendingRequests.Add((pluginId, version), pendingRequest);
        return pendingRequest.Task;
    }

    private void MergePluginInfo(IPluginInfo pluginInfo, IPluginRepository newRepo)
    {
        lock (_pluginInfosLock)
        {
            if (!_pluginInfos.TryGetValue(pluginInfo.Id, out var masterInfo))
            {
                masterInfo = new PluginInfo(pluginInfo.Id, []);
                _pluginInfos.Add(pluginInfo.Id, masterInfo);
            }

            foreach (var version in pluginInfo.AllVersions)
            {
                masterInfo.AddVersion(version, newRepo);
                if (_pendingRequests.TryGetValue((pluginInfo.Id, version), out var pendingRequest))
                {
                    pendingRequest.SetResult(masterInfo);
                }
            }
        }
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
        
        _pluginRepositories.Remove(repository);
    }

    public bool TryGetPluginInfoFromId(string id, [NotNullWhen(true)] out IPluginInfo? pluginInfo)
        => _pluginInfos.TryGetValue(id, out pluginInfo);

    public IReadOnlyList<IPluginRepository> Repositories => _pluginRepositories;

    public IReadOnlyObservableCollection<IPluginRepository> CurrentlyLoadingRepositories => _loadingRepositories.ToInterfaceImpl();
    
    public IReadOnlyObservableCollection<IPluginInfo> LoadedPlugins => _loadedPlugins.ToInterfaceImpl();
    
    public Task EnsurePluginsLoaded() => _loadedCompletionSource?.Task ?? Task.CompletedTask;

}