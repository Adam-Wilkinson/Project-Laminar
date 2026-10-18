using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class PluginManager(
    IWorkspace host, 
    IPluginLibrary library,
    IExceptionHandler exceptionHandler) 
    : IPluginManager
{
    private readonly ObservableDictionary<string, IPluginInstallation> _userInstalledPluginsById = [];
    private readonly Dictionary<string, IObservableValue<SemanticVersion?>> _pluginVersionObservables = [];
    
    public IReadOnlyObservableBag<string> UserInstalledPlugins => _userInstalledPluginsById.Keys;

    public IWorkspace Host => host;

    public event PluginChangedEventHandler? PluginChanged;
    
    public SemanticVersion? GetPluginVersion(string pluginId) 
        => _userInstalledPluginsById.TryGetValue(pluginId, out var plugin) ? plugin.PluginId.Version : null;

    public IReadOnlyObservableValue<SemanticVersion?> GetPluginVersionObservable(string pluginId)
    {
        if (_pluginVersionObservables.TryGetValue(pluginId, out var versionObservable))
        {
            return versionObservable;
        }
        
        var newValue = new ObservableValue<SemanticVersion?>(GetPluginVersion(pluginId));
        _pluginVersionObservables.Add(pluginId, newValue);
        return newValue;
    }

    public async Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        var oldVersion = GetPluginVersion(pluginId.Name);
        if (oldVersion is { } installedPluginVersion && installedPluginVersion == pluginId.Version)
        {
            return true;
        }

        if (await library.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
            await exceptionHandler.OnExceptionAsync(new Exception($"Plugin library could not find plugin {pluginId}"));
            return false;
        }

        if (_userInstalledPluginsById.TryGetValue(pluginId.Name, out var plugin))
        {
            plugin.Uninstall();
            _userInstalledPluginsById.Remove(pluginId.Name);
        }
        
        if (!(await pluginSource.InstallPlugin(pluginId, Host)).TryGetResult(out var newPlugin, out var ex))
        {
            if (oldVersion.HasValue && !await EnsurePluginInstalled(pluginId with { Version = oldVersion.Value }))
            {
                await exceptionHandler.OnExceptionAsync(
                    new Exception("Unable to install new plugin version or roll back to old version", ex));
                return false;
            }
            
            await exceptionHandler.OnExceptionAsync(ex);
            return false;
        }
        
        _userInstalledPluginsById[pluginId.Name] = newPlugin;
        NotifyPluginVersionChange(pluginId.Name, oldVersion, pluginId.Version);
        return true;
    }

    public void UninstallPlugin(string pluginId)
    {
        if (!_userInstalledPluginsById.TryGetValue(pluginId, out var plugin))
        {
            return;
        }
        
        plugin.Uninstall();
        _userInstalledPluginsById.Remove(pluginId);
        NotifyPluginVersionChange(pluginId, plugin.PluginId.Version, null);
    }

    private void NotifyPluginVersionChange(string pluginId, SemanticVersion? oldVersion, SemanticVersion? newVersion)
    {
        PluginChanged?.Invoke(this, new PluginChangedEventArgs
        {
            ChangedWorkspace = Host,
            PluginId = pluginId,
            ChangeType = (oldVersion, newVersion) switch
            {
                (null, _) => PluginChangedType.Added,
                (_, null) => PluginChangedType.Removed,
                (_, _) => PluginChangedType.VersionChanged,
            }
        });
        
        if (_pluginVersionObservables.TryGetValue(pluginId, out var observable))
        {
            observable.Value = newVersion;
        }
    }
}