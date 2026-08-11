using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public IReadOnlyItemCategory<ILoadedNodeInfo> LoadedNodes { get; }
    
    public ILoadedNodeInfo? GetInfoFrom(NodeId nodeId);

    public void AddNodeToCategory(string categoryPath, ILoadedNodeInfo newNodeInfo);
    
    public IWrappedNode CreateNode(IPersistentDictionary persistentDictionary);
    
    public IWrappedNode CreateNode(ILoadedNodeInfo loadedNodeInfo);
}

public record struct NodeId(VersionedPluginId Plugin, string NodeName);