using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager : ILoadedNodeManager
{
    private readonly ItemCategory<ILoadedNodeInfo> _writableLoadedNodes = new ("root");
    private readonly Dictionary<Type, ILoadedNodeInfo> _loadedNodes = [];
    
    public IReadOnlyItemCategory<ILoadedNodeInfo> LoadedNodes => _writableLoadedNodes;

    public ILoadedNodeInfo? GetInfoFrom(Type nodeType) => _loadedNodes.GetValueOrDefault(nodeType);

    public void AddNodeToCategory<TNode>(string categoryPath, IRegisteredPlugin pluginHost)
        where TNode : INode, new()
    {
        var newNodeInfo = new LoadedNodeInfo<TNode>(pluginHost);
        _loadedNodes.Add(typeof(TNode), newNodeInfo);
        _writableLoadedNodes.AddItem(newNodeInfo, categoryPath);
    }
}
