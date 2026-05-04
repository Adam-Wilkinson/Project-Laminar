using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public ItemCategory<IWrappedNode> LoadedNodes { get; }

    public void AddNodeToCategory<TNode>(TNode newNode, string categoryPath) where TNode : INode, new();
}
