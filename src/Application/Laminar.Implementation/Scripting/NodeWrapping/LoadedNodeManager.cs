using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager(INodeFactory nodeFactory) : ILoadedNodeManager
{
    private readonly ItemCategory<ILoadedNodeInfo> _writableLoadedNodes = new ("root");
    
    public IReadOnlyItemCategory<ILoadedNodeInfo> LoadedNodes => _writableLoadedNodes;

    public void AddNodeToCategory<TNode>(string categoryPath, IRegisteredPlugin pluginHost)
        where TNode : INode, new()
    {
        _writableLoadedNodes.AddItem(new LoadedNodeInfo<TNode>(pluginHost, nodeFactory), categoryPath);
    }
}
