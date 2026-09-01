using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager(IRuntimeHost host, INodeFactory nodeFactory) : ILoadedNodeManager
{
    private readonly ItemCategory<NodeDescriptor> _writableLoadedNodes = new ("root");
    private readonly Dictionary<NodeDescriptor, ILoadedNodeInfo> _loadedNodeInfos = [];
    
    public IReadOnlyItemCategory<NodeDescriptor> LoadedNodes => _writableLoadedNodes;
    
    public ILoadedNodeInfo? GetInfoFrom(NodeDescriptor nodeDescriptor) => _loadedNodeInfos.TryGetValue(nodeDescriptor, out var info) ? info : null;

    public void AddNodeToCategory(string categoryPath, ILoadedNodeInfo newNodeInfo)
    {
        _loadedNodeInfos.Add(newNodeInfo.NodeDescriptor, newNodeInfo);
        _writableLoadedNodes.AddItem(newNodeInfo.NodeDescriptor, categoryPath);
    }

    public INodeContainer CreateNode(IPersistentDictionary persistentDictionary)
        => nodeFactory.FromPersistentData(persistentDictionary, host);

    public INodeContainer CreateNode(NodeDescriptor nodeDescriptor)
        => nodeFactory.FromDescriptor(nodeDescriptor, host);
}
