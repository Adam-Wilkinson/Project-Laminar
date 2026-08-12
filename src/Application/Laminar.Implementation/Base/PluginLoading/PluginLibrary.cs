using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginLibrary(IExceptionHandler exceptionHandler) : IPluginLibrary
{
    private readonly List<IPluginSource> _pluginRepositories = [];
    private readonly Dictionary<string, IPluginInfo> _pluginInfos = [];
    private readonly Dictionary<VersionedPluginId, TaskCompletionSource<IPluginInfo?>> _pendingRequests = [];
    private readonly ObservableCollection<IPluginInfo> _loadedPlugins = [];
    private readonly ObservableCollection<IPluginSource> _loadingRepositories = [];
    private readonly Lock _loadingRepositoriesLock = new();
    private readonly Lock _pluginInfosLock = new();
    
    private TaskCompletionSource? _loadedCompletionSource;
    
    public IReadOnlyList<IPluginSource> Sources => _pluginRepositories;

    public IReadOnlyObservableCollection<IPluginSource> CurrentlyLoadingSources => field ??= _loadingRepositories.ToInterfaceImpl();
    
    public IReadOnlyObservableCollection<IPluginInfo> LoadedPlugins => field ??= _loadedPlugins.ToInterfaceImpl();
    
    public async Task AddSource(IPluginSource source)
    {
        _pluginRepositories.Add(source);
        lock (_loadingRepositoriesLock)
        {
            if (_loadingRepositories.Count == 0)
            {
                _loadedCompletionSource = new TaskCompletionSource();
            }
            _loadingRepositories.Add(source);   
        }

        try
        {
            await foreach (var pluginInfo in source.Reload())
            {
                MergePluginInfo(pluginInfo, source);
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
                OnRepositoryLoadFinished(source);
            } 
        }
    }

    public Task<IPluginInfo?> GetPluginInfoOrNull(VersionedPluginId pluginId)
    {
        if (_pluginInfos.TryGetValue(pluginId.Name, out var pluginInfo) && pluginInfo.HasVersion(pluginId.Version))
        {
            return Task.FromResult<IPluginInfo?>(pluginInfo);
        }

        if (EnsurePluginsLoaded().IsCompleted)
        {
            return Task.FromResult<IPluginInfo?>(null);
        }
        
        if (_pendingRequests.TryGetValue(pluginId, out var pendingRequest))
        {
            return pendingRequest.Task;
        }
        
        pendingRequest = new TaskCompletionSource<IPluginInfo?>();
        _pendingRequests.Add(pluginId, pendingRequest);
        return pendingRequest.Task;
    }

    private void MergePluginInfo(VersionedPluginId pluginId, IPluginSource newRepo)
    {
        lock (_pluginInfosLock)
        {
            if (!_pluginInfos.TryGetValue(pluginId.Name, out var masterInfo))
            {
                masterInfo = new PluginInfo(pluginId.Name);
                _loadedPlugins.Add(masterInfo);
                _pluginInfos.Add(pluginId.Name, masterInfo);
            }

            masterInfo.AddVersion(pluginId.Version, newRepo);
            if (_pendingRequests.TryGetValue(pluginId, out var pendingRequest))
            {
                pendingRequest.SetResult(masterInfo);
            }
        }
    }

    public void ForgetSource(IPluginSource source)
    {
        foreach (var (id, version) in source.Plugins)
        {
            if (!_pluginInfos.TryGetValue(id, out var masterInfo))
            {
                continue;
            }

            masterInfo.RemoveVersion(version, source);
            if (masterInfo.AllVersions.Count == 0)
            {
                _pluginInfos.Remove(id);
            }
        }
        
        _pluginRepositories.Remove(source);
    }

    public bool TryGetPluginInfoFromId(string id, [NotNullWhen(true)] out IPluginInfo? pluginInfo)
        => _pluginInfos.TryGetValue(id, out pluginInfo);
    
    public Task EnsurePluginsLoaded() => _loadedCompletionSource?.Task ?? Task.CompletedTask;
    
    private void OnRepositoryLoadFinished(IPluginSource newRepo)
    {
        _loadingRepositories.Remove(newRepo);
        if (_loadingRepositories.Count != 0) return;
        
        _loadedCompletionSource?.SetResult();
        _loadedCompletionSource = null;

        foreach (var incompleteRequest in _pendingRequests.Values)
        {
            incompleteRequest.SetResult(null);
        }
        _pendingRequests.Clear();
    }
}