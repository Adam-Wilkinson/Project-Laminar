using Laminar.Contracts.Base.PluginLoading;

namespace Laminar.Implementation.Base.PluginLoading;

public class PluginRegistry : IWritablePluginRegistry
{
    private readonly Dictionary<string, IRegisteredPlugin> _plugins = [];

    public IRegisteredPlugin GetPluginFromName(string pluginName) => _plugins[pluginName];

    public void RegisterPlugin(IRegisteredPlugin plugin)
    {
        _plugins.Add(plugin.PluginName, plugin);
    }

    public void UnregisterPlugin(IRegisteredPlugin plugin)
    {
        _plugins.Remove(plugin.PluginName);
    }
}