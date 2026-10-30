using Laminar.Contracts.Base;
using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Workspaces;
using Laminar.Contracts.Workspaces.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Workspaces.PluginLoading.Actions;

namespace Laminar.Implementation.Workspaces.PluginLoading;

internal sealed class PluginManager(
    IWorkspace host, 
    IPluginLibrary library,
    IExceptionHandler exceptionHandler) 
    : IPluginManager
{
    private readonly ObservableDictionary<string, IPluginInstallation> _userInstalledPluginsById = [];
    private readonly Dictionary<string, IObservableValue<SemanticVersion?>> _pluginVersionObservables = [];

    private PluginActionContext Context => field ??= new PluginActionContext(host, library, _userInstalledPluginsById);

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

    public async Task<UserActionResult> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        var currentVersion = GetPluginVersion(pluginId.Name);
        var result = await Host.ActionScope.ExecuteAction(new InstallPluginAction(pluginId, Context));
        if (!result.Succeeded)
        {
            await exceptionHandler.OnExceptionsAsync(result.Exceptions);
            return result;
        }
        
        NotifyPluginVersionChange(pluginId.Name, currentVersion, pluginId.Version);
        return result;
    }

    public async Task<UserActionResult> UninstallPlugin(string pluginId)
    {
        var currentVersion = GetPluginVersion(pluginId);
        var result = await Host.ActionScope.ExecuteAction(new UninstallPluginAction(pluginId, Context));
        if (!result.Succeeded)
        {
            await exceptionHandler.OnExceptionsAsync(result.Exceptions);
            return result;
        }
        
        NotifyPluginVersionChange(pluginId, currentVersion, null);
        return result;
    }

    private void NotifyPluginVersionChange(string pluginId, SemanticVersion? oldVersion, SemanticVersion? newVersion)
    {
        if (Equals(oldVersion, newVersion)) return;
        
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