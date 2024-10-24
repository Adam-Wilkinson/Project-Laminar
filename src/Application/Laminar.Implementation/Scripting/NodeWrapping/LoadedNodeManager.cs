using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class LoadedNodeManager : ILoadedNodeManager
{
    private readonly INodeFactory _nodeFactory;

    public LoadedNodeManager(INodeFactory nodeFactory)
    {
        _nodeFactory = nodeFactory;
    }

    public ItemCatagory<IWrappedNode> LoadedNodes { get; } = new("root");

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath)
        where TNode : INode, new()
    {
        IWrappedNode container = _nodeFactory.WrapNode(newNode, null);
        LoadedNodes.AddItem(container, catagoryPath);
    }
}
