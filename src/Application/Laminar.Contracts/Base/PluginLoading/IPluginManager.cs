using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginManager
{
    public IReadOnlyObservableBag<IInstalledPlugin> UserInstalledPlugins { get; }
    
    public IReadOnlyObservableBag<IInstalledPlugin> ReferencedPlugins { get; }

    public Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId);

    public bool TryGetInstalledPlugin(VersionedPluginId pluginId, [NotNullWhen(true)] out IInstalledPlugin? plugin);
    
    void UninstallPlugin(VersionedPluginId pluginId);
}