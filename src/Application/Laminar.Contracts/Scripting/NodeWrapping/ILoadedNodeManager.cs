using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain;
using Laminar.Domain.ValueObjects;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public IReadOnlyItemCategory<NodeDescriptor> LoadedNodes { get; }
    
    public ILoadedNodeInfo? GetInfoFrom(NodeDescriptor nodeDescriptor);

    public void AddNodeToCategory(string categoryPath, ILoadedNodeInfo newNodeInfo);
    
    public INodeContainer CreateNode(IPersistentDictionary persistentDictionary);
    
    public INodeContainer CreateNode(NodeDescriptor nodeDescriptor);
}

public record struct NodeDescriptor(VersionedPluginId Plugin, string NodeName);