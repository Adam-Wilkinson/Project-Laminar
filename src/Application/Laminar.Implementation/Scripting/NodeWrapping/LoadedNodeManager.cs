using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager(INodeFactory nodeFactory) : ILoadedNodeManager
{
    private readonly ItemCategory<ILoadedNodeInfo> _writableLoadedNodes = new ("root");
    private readonly Dictionary<NodeId, ILoadedNodeInfo> _loadedNodeInfos = [];
    
    public IReadOnlyItemCategory<ILoadedNodeInfo> LoadedNodes => _writableLoadedNodes;
    
    public ILoadedNodeInfo? GetInfoFrom(NodeId nodeId) => _loadedNodeInfos.TryGetValue(nodeId, out var info) ? info : null;

    public void AddNodeToCategory(string categoryPath, ILoadedNodeInfo newNodeInfo)
    {
        _loadedNodeInfos.Add(new NodeId(newNodeInfo.PluginId, newNodeInfo.Id), newNodeInfo);
        _writableLoadedNodes.AddItem(newNodeInfo, categoryPath);
    }

    public IWrappedNode CreateNode(IPersistentDictionary persistentDictionary)
        => nodeFactory.FromPersistentData(persistentDictionary, this);

    public IWrappedNode CreateNode(ILoadedNodeInfo loadedNodeInfo)
        => nodeFactory.FromNodeInfo(loadedNodeInfo, this);
}
