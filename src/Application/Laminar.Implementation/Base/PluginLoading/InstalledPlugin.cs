using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class InstalledPlugin : IInstalledPlugin
{
    private readonly IPluginHost _host;

    public InstalledPlugin(IPluginHostFactory hostFactory, ILoadedNodeManager nodeManager)
    {
        _host = hostFactory.GetPluginHost(this, nodeManager);
    }

    public VersionedPluginId Id { get; init; }
    
    public void AddPluginImplementation(IPlugin plugin)
    {
        plugin.Register(_host);
    }
}