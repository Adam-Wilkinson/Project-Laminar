using Laminar.Contracts.NodeSystem;
using Laminar.Domain;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar.Core.ScriptEditor.Nodes;

public class LoadedNodeManager : ILoadedNodeManager
{
    private readonly INodeFactory _nodeFactory;

    public LoadedNodeManager(INodeFactory nodeFactory)
    {
        _nodeFactory = nodeFactory;
    }

    public ItemCatagory<INodeWrapper> LoadedNodes { get; } = new("root");

    public void AddNodeToCatagory<TNode>(TNode newNode, string catagoryPath)
        where TNode : INode, new()
    {
        INodeWrapper container = _nodeFactory.WrapNode(newNode, null);
        LoadedNodes.AddItem(container, catagoryPath);
    }
}
