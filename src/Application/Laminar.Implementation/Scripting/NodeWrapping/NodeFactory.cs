using System.Linq;
using Laminar.Contracts.Notification;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeFactory : INodeFactory
{
    public IWrappedNode WrapNode<T>(T node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        return new WrappedNode<T>(CreateNameRowFor(node), this, node) { UserChangedValueNotificationClient = userChangedValueNotificationClient };
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    public IWrappedNode CloneNode<T>(IWrappedNode nodeToCopy, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        var newNode = (WrappedNode<T>)WrapNode<T>(null);

        foreach (var (copyFrom, copyTo) in nodeToCopy.Rows.Zip(newNode.Rows))
        {
            copyFrom.CopyValueTo(copyTo);
        }

        newNode.Location.Value = nodeToCopy.Location.Value;
        newNode.UserChangedValueNotificationClient = userChangedValueNotificationClient;
        return newNode;
    }

    private static INodeRow CreateNameRowFor(INode node)
    {
        var nameLabel = LaminarFactory.NodeIO.ValueInput("", node.NodeName);
        nameLabel.InterfaceDefinition.Editor = new EditableLabel();
        return LaminarFactory.Component.CreateSingleRow(null, nameLabel.DisplayValue, null);
    }
}
