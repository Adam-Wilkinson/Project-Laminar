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

public class NodeFactory(IEncodableDataFactory dataFactory, ILoadedNodeManager loadedNodeManager, IPluginRegistry pluginRegistry) : INodeFactory
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
        
        var node = loadedNodeInfo.CreateInstance();
        
        var nameRow = LaminarFactory.Component.CreateSingleRow(null, new ObservableValueInterfaceData<EditableLabel, string>(persistentDictionary["Name"].GetValueOrInitialize(node.NodeName))
        {
            Name = "",
            Definition = new EditableLabel()
        }, null);
        
        return new WrappedNode(node, persistentDictionary)
        {
            NameRow = nameRow,
            Info = loadedNodeInfo,
            UserChangedValueNotificationClient = userChangedValueClient
        };
    }

    public IWrappedNode FromNodeInfo(ILoadedNodeInfo loadedNode, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        ArgumentNullException.ThrowIfNull(loadedNode.NodeType.FullName);
        var persistentDictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
        persistentDictionary[PluginKey].GetValueOrInitialize(loadedNode.Plugin.PluginName);
        persistentDictionary[TypeKey].GetValueOrInitialize(loadedNode.NodeType.FullName);
        return FromPersistentData(persistentDictionary, userChangedValueNotificationClient);
    }

    private record PersistentNodeModel(string PluginName, string NodeType);
}
