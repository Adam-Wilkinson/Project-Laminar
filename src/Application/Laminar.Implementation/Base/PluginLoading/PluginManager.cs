using System.Collections.ObjectModel;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginManager(
    IRuntimeHost host, 
    ISharedPluginContext context,
    IPluginLibrary library) 
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
        
        var newPlugin = await pluginSource.InstallPlugin(pluginId, host);
        _plugins.Add(newPlugin);
        _installedPlugins.Add(pluginId, newPlugin);
        context.RegisterInstallation(newPlugin);
        return newPlugin;
    }
}