using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginManager
{
    public IReadOnlyObservableBag<VersionedPluginId> UserInstalledPlugins { get; }
    
    public IRuntimeHost Host { get; }
    
    public Task<bool> EnsurePluginInstalled(VersionedPluginId pluginId);

    public bool PluginInstalled(VersionedPluginId pluginId);
    
    void UninstallPlugin(VersionedPluginId pluginId);
}