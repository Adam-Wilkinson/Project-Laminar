using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.Nodes;

public class NodeFactory : INodeFactory
{
    private readonly INodeRowCollectionFactory _rowCollectionFactory;
    private readonly INodeComponentFactory _rowFactory;
    private readonly INotifyCollectionChangedHelper _collectionHelper;

    public NodeFactory(
        INodeRowCollectionFactory rowCollectionFactory, 
        INodeComponentFactory rowFactory, 
        INotifyCollectionChangedHelper collectionHelper)
    {
        _rowCollectionFactory = rowCollectionFactory;
        _rowFactory = rowFactory;
        _collectionHelper = collectionHelper;
    }

    public IWrappedNode WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        return new WrappedNode<T>(CreateNameRowFor(node), _rowCollectionFactory, this, node, _collectionHelper) { UserChangedValueNotificationClient = userChangedValueNotificationClient };
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    public IWrappedNode CloneNode<T>(IWrappedNode nodeToCopy, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        WrappedNode<T> newNode = (WrappedNode<T>)WrapNode<T>(null);
        _rowCollectionFactory.CopyNodeRowValues(nodeToCopy, newNode);
        newNode.Location.Value = nodeToCopy.Location.Value;
        newNode.UserChangedValueNotificationClient = userChangedValueNotificationClient;
        return newNode;
    }

    private INodeRow CreateNameRowFor(INode node)
    {
        IValueInput<string> nameLabel = PluginFramework.NodeSystem.IO.NodeIO.ValueInput("", node.NodeName);
        nameLabel.ValueUserInterface.Editor = new EditableLabel();
        return _rowFactory.CreateNodeRow(null, nameLabel, null);
    }
}
