using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Implementation.Base.UserInterface;
using Laminar.PluginFramework;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeFactory(IEncodableDataFactory dataFactory) : INodeFactory
{
    public IWrappedNode CreateMatchingNode(IWrappedNode node, INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
    {
        if (node is not WrappedNode wrapped || Activator.CreateInstance(wrapped.CoreNode.GetType()) is not INode newNode) 
            throw new InvalidOperationException();
        
        return WrapNode(newNode, userChangedValueClient);
    }

    public IWrappedNode WrapNode(INode node, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        var persistentDictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
        return new WrappedNode(CreateNameRowFor(node, persistentDictionary), node, persistentDictionary)
        {
            UserChangedValueNotificationClient = userChangedValueNotificationClient
        };
    }

    public IWrappedNode WrapNode<T>(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient) where T : INode, new()
    {
        T output = new();
        return WrapNode(output, userChangedValueNotificationClient);
    }

    private static INodeRow<IInterfaceData<EditableLabel, string>> CreateNameRowFor(INode node, IPersistentDictionary persistentDictionary) => 
        LaminarFactory.Component.CreateSingleRow(null, persistentDictionary.GetValueInterface(node.NodeName, "", new EditableLabel()), null);
}
