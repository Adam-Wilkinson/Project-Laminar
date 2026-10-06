using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginManager
{
    public IRuntimeHost Host { get; }
    
    public IReadOnlyObservableBag<string> UserInstalledPlugins { get; }
    
    public Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId);
    
    public void UninstallPlugin(string pluginId);
    
    public SemanticVersion? GetInstalledPluginVersion(string pluginId);
}