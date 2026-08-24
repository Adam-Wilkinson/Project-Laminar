using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class InstalledPlugin : IInstalledPlugin
{
    private readonly IPluginHost _pluginHost;
    private readonly List<IPlugin> _implementingTypes = [];

    public InstalledPlugin(
        IPluginHostFactory pluginHostFactory, 
        IRuntimeHost host,
        VersionedPluginId pluginId)
    {
        _pluginHost = pluginHostFactory.GetPluginHost(this, host.NodeManager);
        Host = host;
        PluginId = pluginId;
    }

    public void AddPluginImplementation(IPlugin plugin)
    {
        plugin.Register(_pluginHost);
        _implementingTypes.Add(plugin);
    }

    public VersionedPluginId PluginId { get; }
    
    public IRuntimeHost Host { get; }

    public IReadOnlyList<IPlugin> ImplementingTypes => _implementingTypes;
}