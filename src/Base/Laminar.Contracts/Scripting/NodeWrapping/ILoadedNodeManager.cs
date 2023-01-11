using Laminar.Contracts.Primitives;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public ItemCatagory<IWrappedNode> LoadedNodes { get; }

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath) where TNode : INode, new();
}
