using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeFactory : INodeFactory
{
    public IWrappedNode CreateMatchingNode(IWrappedNode node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
    {
        if (node is not WrappedNode wrapped || Activator.CreateInstance(wrapped.CoreNode.GetType()) is not INode newNode) 
            throw new InvalidOperationException();
        
        return WrapNode(newNode, userChangedValueClient);
    }

    public IWrappedNode WrapNode(INode node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        return new WrappedNode(CreateNameRowFor(node), node) { UserChangedValueNotificationClient = userChangedValueNotificationClient };
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    private static INodeRow CreateNameRowFor(INode node) => 
        LaminarFactory.Component.CreateSingleRow(null, 
            new InterfaceData<EditableLabel, string>
            {
                Name = "", 
                Definition = new EditableLabel(), 
                Value = node.NodeName
            }, null);
}
