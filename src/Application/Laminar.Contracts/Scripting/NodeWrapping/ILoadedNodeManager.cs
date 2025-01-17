using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.NodeWrapping;

public interface ILoadedNodeManager
{
    public ItemCategory<IWrappedNode> LoadedNodes { get; }

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath) where TNode : INode, new();
}
