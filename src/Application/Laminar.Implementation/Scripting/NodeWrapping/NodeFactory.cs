using System.Linq;
using Laminar.Contracts.Notification;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.IO.Value;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.Nodes;

public class NodeFactory : INodeFactory
{
    private readonly INotifyCollectionChangedHelper _collectionHelper;

    public NodeFactory(
        INotifyCollectionChangedHelper collectionHelper)
    {
        _collectionHelper = collectionHelper;
    }

    public IWrappedNode WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        return new WrappedNode<T>(CreateNameRowFor(node), this, node, _collectionHelper) { UserChangedValueNotificationClient = userChangedValueNotificationClient };
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    public IWrappedNode CloneNode<T>(IWrappedNode nodeToCopy, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        WrappedNode<T> newNode = (WrappedNode<T>)WrapNode<T>(null);

        foreach ((INodeRow copyFrom, INodeRow copyTo) in nodeToCopy.Rows.Zip(newNode.Rows))
        {
            copyFrom.CopyValueTo(copyTo);
        }

        newNode.Location.Value = nodeToCopy.Location.Value;
        newNode.UserChangedValueNotificationClient = userChangedValueNotificationClient;
        return newNode;
    }

    private INodeRow CreateNameRowFor(INode node)
    {
        IValueInput<string> nameLabel = LaminarFactory.NodeIO.ValueInput("", node.NodeName);
        nameLabel.InterfaceDefinition.Editor = new EditableLabel();
        return LaminarFactory.Component.CreateSingleRow(null, nameLabel.DisplayValue, null);
    }
}
