using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginManager(
    IRuntimeHost host, 
    ISharedPluginContext context,
    IPluginLibrary library,
    IExceptionHandler exceptionHandler) 
    : IPluginManager
{
    private readonly ObservableCollection<IInstalledPlugin> _plugins = [];
    private readonly Dictionary<VersionedPluginId, IInstalledPlugin> _installedPlugins = [];
    
    public IReadOnlyObservableCollection<IInstalledPlugin> Plugins => field ??= _plugins.ToInterfaceImpl();
    
    public async Task<IInstalledPlugin?> EnsurePluginInstalled(VersionedPluginId pluginId)
    {
        if (_installedPlugins.TryGetValue(pluginId, out var plugin))
        {
            return plugin;
        }

        if (await library.GetPluginSourceOrNull(pluginId) is not { } pluginSource)
        {
            return null;
        }

        if (!(await pluginSource.InstallPlugin(pluginId, host)).TryGetResult(out var newPlugin, out var ex))
        {
            await exceptionHandler.OnExceptionAsync(ex);
            return null;
        }
        
        _plugins.Add(newPlugin);
        _installedPlugins.Add(pluginId, newPlugin);
        context.RegisterInstallation(newPlugin);
        return newPlugin;
    }

    public bool TryGetInstalledPlugin(VersionedPluginId pluginId, [NotNullWhen(true)] out IInstalledPlugin? plugin)
        => _installedPlugins.TryGetValue(pluginId, out plugin);
}