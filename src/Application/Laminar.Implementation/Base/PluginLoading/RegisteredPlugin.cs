using System.Reflection;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.Registration;

namespace Laminar.Implementation.Base.PluginLoading;

internal sealed class RegisteredPlugin : IRegisteredPlugin
{
    private readonly IPluginHost _host;
    private readonly IPlugin _plugin;
    private readonly HashSet<Type> _registeredNodes = [];

    public RegisteredPlugin(IPlugin plugin, IPluginHostFactory pluginHostFactory, Assembly runtimeAssembly)
    {
        _host = pluginHostFactory.GetPluginHost(this);
        _plugin = plugin;
        PluginName = plugin.PluginName;
        PluginDescription = plugin.PluginDescription;
        RuntimeAssembly = runtimeAssembly;
    }

    public string PluginName { get; }

    public string PluginDescription { get; }
    
    public Assembly RuntimeAssembly { get; }

    public void RegisterNode<TNode>() where TNode : INode, new() => _registeredNodes.Add(typeof(TNode));

    public bool ContainsNode(INode node) => _registeredNodes.Contains(node.GetType());
    
    public void Load()
    {
        _plugin.Register(_host);
    }

    public void Unload()
    {
        throw new NotImplementedException();
    }
}