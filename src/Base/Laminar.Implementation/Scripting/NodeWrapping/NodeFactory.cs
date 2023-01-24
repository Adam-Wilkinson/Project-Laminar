using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.UserInterfaces;

namespace Laminar.Implementation.Scripting.Nodes;

public class NodeFactory : INodeFactory
{
    private readonly INodeRowCollectionFactory _rowCollectionFactory;
    private readonly INodeRowWrapperFactory _rowFactory;

    public NodeFactory(INodeRowCollectionFactory rowCollectionFactory, INodeRowWrapperFactory rowFactory)
    {
        _rowCollectionFactory = rowCollectionFactory;
        _rowFactory = rowFactory;
    }

    public IWrappedNode WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        return new WrappedNode<T>(CreateNameRowFor(node), _rowCollectionFactory, userChangedValueNotificationClient, this, node);
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    public IWrappedNode CloneNode<T>(IWrappedNode nodeToCopy, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        IWrappedNode newNode = WrapNode<T>(userChangedValueNotificationClient);
        _rowCollectionFactory.CopyNodeRowValues(nodeToCopy, newNode);
        newNode.Location.Value = nodeToCopy.Location.Value;
        return newNode;
    }

    private IWrappedNodeRow CreateNameRowFor(INode node)
    {
        return _rowFactory.CreateNodeRowWrapper(new NodeRow(null, new ValueInput<string>("", node.NodeName) { Editor = new EditableLabel() }, null), null);
    }
}
