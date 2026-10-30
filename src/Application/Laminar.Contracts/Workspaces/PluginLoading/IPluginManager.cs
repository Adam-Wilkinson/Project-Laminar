using Laminar.Contracts.Base.ActionSystem;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Workspaces.PluginLoading;

public interface IPluginManager
{
    public IWorkspace Host { get; }
    
    public IReadOnlyObservableBag<string> UserInstalledPlugins { get; }
    
    public event PluginChangedEventHandler? PluginChanged;
    
    public Task<UserActionResult> EnsurePluginInstalled(VersionedPluginId pluginId);
    
    public Task<UserActionResult> UninstallPlugin(string pluginId);
    
    public SemanticVersion? GetPluginVersion(string pluginId);

    public IReadOnlyObservableValue<SemanticVersion?> GetPluginVersionObservable(string pluginId);
}