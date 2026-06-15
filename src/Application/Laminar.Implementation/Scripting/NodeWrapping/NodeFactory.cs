using Laminar.Contracts.Base.PluginLoading;
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

public class NodeFactory(
    IEncodableDataFactory dataFactory,
    ILoadedNodeManager loadedNodeManager,
    IPluginRegistry pluginRegistry) : INodeFactory
{
    private const string PluginKey = "Plugin";
    private const string TypeKey = "Type";

    public IWrappedNode FromPersistentData(IPersistentDictionary persistentDictionary,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
    {
        string pluginName = persistentDictionary[PluginKey].GetValue<string>().Value;
        string nodeTypeName = persistentDictionary[TypeKey].GetValue<string>().Value;
        
        var plugin = pluginRegistry.GetPluginFromName(pluginName);
        var pluginType = plugin.RuntimeAssembly.GetType(nodeTypeName);

        if (pluginType is null)
        {
            throw new InvalidOperationException($"Unable to find node type {nodeTypeName} in plugin {pluginName}");
        }
        
        if (loadedNodeManager.GetInfoFrom(pluginType) is not { } loadedNodeInfo)
        {
            throw new InvalidOperationException($"Unable to get node info for type {pluginType}");
        }
        
        return FromNodeInfo(loadedNodeInfo, userChangedValueClient);
    }
    
    public IWrappedNode FromNodeInfo(ILoadedNodeInfo nodeInfo,
        INotificationClient<LaminarExecutionContext>? userChangedValueClient = null)
    {
        return WrapNode(nodeInfo.CreateInstance(), userChangedValueClient);
    }
    
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
        LaminarFactory.Component.CreateSingleRow(null, new ObservableValueInterfaceData<EditableLabel, string>(persistentDictionary["Name"].GetValueOrDefault(node.NodeName))
        {
            Name = "",
            Definition = new EditableLabel()
        }, null);
}
