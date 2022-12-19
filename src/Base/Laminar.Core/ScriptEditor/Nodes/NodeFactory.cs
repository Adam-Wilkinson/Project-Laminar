using System.Collections.ObjectModel;
using System.Reflection;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;
using Laminar_PluginFramework.UserInterfaces;

namespace Laminar.Core.ScriptEditor.Nodes;

public class NodeFactory : INodeFactory
{
    private readonly INodeRowCollectionFactory _rowCollectionFactory;
    private readonly INodeRowWrapperFactory _rowFactory;

    public NodeFactory(INodeRowCollectionFactory rowCollectionFactory, INodeRowWrapperFactory rowFactory)
    {
        _rowCollectionFactory = rowCollectionFactory;
        _rowFactory = rowFactory;
    }

    public INodeWrapper WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        return new NodeWrapper<T>(CreateNameRowFor(node), _rowCollectionFactory, userChangedValueNotificationClient, this, node);
    }

    public INodeWrapper WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    public INodeWrapper CloneNode<T>(INodeWrapper nodeToCopy, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        INodeWrapper newNode = WrapNode<T>(userChangedValueNotificationClient);
        _rowCollectionFactory.CopyNodeRowValues(nodeToCopy, newNode);
        newNode.Location.Value = nodeToCopy.Location.Value;
        return newNode;
    }

    private INodeRowWrapper CreateNameRowFor(INode node)
    {
        return _rowFactory.CreateNodeRowWrapper(new NodeRow(null, new ValueInput<string>("", node.NodeName) { Editor = new EditableLabel() }, null), null);
    }
}
