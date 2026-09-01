using System.Diagnostics.CodeAnalysis;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Base.PluginLoading;

public interface IPluginManager
{
    public IReadOnlyObservableCollection<IInstalledPlugin> Plugins { get; }

    public Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId);

    public bool TryGetInstalledPlugin(VersionedPluginId pluginId, [NotNullWhen(true)] out IInstalledPlugin? plugin);
}