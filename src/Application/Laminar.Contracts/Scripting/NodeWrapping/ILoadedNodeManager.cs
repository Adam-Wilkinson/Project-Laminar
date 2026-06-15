using Laminar.Contracts.Base.PluginLoading;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public IReadOnlyItemCategory<ILoadedNodeInfo> LoadedNodes { get; }

    public ILoadedNodeInfo? GetInfoFrom(Type nodeType);
    
    public void AddNodeToCategory<TNode>(string categoryPath, IRegisteredPlugin pluginHost) where TNode : INode, new();
}
