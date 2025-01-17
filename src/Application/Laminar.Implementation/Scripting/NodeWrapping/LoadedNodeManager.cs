using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager(INodeFactory nodeFactory) : ILoadedNodeManager
{
    public ItemCategory<IWrappedNode> LoadedNodes { get; } = new("root");

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath)
        where TNode : INode, new()
    {
        var container = nodeFactory.WrapNode(newNode, null);
        LoadedNodes.AddItem(container, catagoryPath);
    }
}
