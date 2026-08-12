using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class InstalledPlugin : IInstalledPlugin
{
    private readonly IPluginHost _pluginHost;

    public InstalledPlugin(
        IPluginHostFactory pluginHostFactory, 
        IRuntimeHost host,
        IPluginInfo pluginInfo,
        SemanticVersion version)
    {
        _pluginHost = pluginHostFactory.GetPluginHost(this, host.NodeManager);
        Host = host;
        PluginInfo = pluginInfo;
        Version = version;
    }

    public void AddPluginImplementation(IPlugin plugin)
    {
        plugin.Register(_pluginHost);
    }

    public IPluginInfo PluginInfo { get; }
    public SemanticVersion Version { get; }
    public IRuntimeHost Host { get; }
}