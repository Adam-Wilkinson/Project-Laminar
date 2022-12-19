using Laminar.Contracts.Primitives;
using Laminar.Domain;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar.Contracts.NodeSystem;

public interface ILoadedNodeManager
{
    public ItemCatagory<INodeWrapper> LoadedNodes { get; }

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath) where TNode : INode, new();
}
