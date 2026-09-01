using System.Diagnostics.CodeAnalysis;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

public class InstalledPlugin : IInstalledPlugin
{
    private readonly IPluginHost _pluginHost;
    private readonly List<IPlugin> _implementingTypes = [];
    private readonly Dictionary<string, ILoadedNodeInfo> _loadedNodeInfos = [];
    
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

    public void AddNode(string name, ILoadedNodeInfo nodeInfo) => _loadedNodeInfos.Add(name, nodeInfo);
    
    public VersionedPluginId PluginId { get; }
    
    public IRuntimeHost Host { get; }
    
    public bool TryGetNodeInfo(string nodeName, [NotNullWhen(true)] out ILoadedNodeInfo? nodeInfo) 
        => _loadedNodeInfos.TryGetValue(nodeName, out nodeInfo);

    public IReadOnlyList<IPlugin> ImplementingTypes => _implementingTypes;
}