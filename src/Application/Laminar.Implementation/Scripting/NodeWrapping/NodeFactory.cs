using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.UserInterface;
using Laminar.PluginFramework;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeFactory(IEncodableDataFactory dataFactory) : INodeFactory
{
    private const string PluginKey = "plugin";
    private const string PluginVersionKey = "plugin-version";
    private const string NodeNameKey = "type";

    public IWrappedNode FromPersistentData(IPersistentDictionary persistentDictionary, ILoadedNodeManager loadedNodeManager)
    {
        var pluginName = persistentDictionary[PluginKey].GetValue<string>().Value;
        
        var pluginVersion = persistentDictionary[PluginVersionKey].GetValue<SemanticVersion>().Value;
        var nodeName = persistentDictionary[NodeNameKey].GetValue<string>().Value;
        VersionedPluginId pluginId = new VersionedPluginId(pluginName, pluginVersion);
        if (loadedNodeManager.GetInfoFrom(new NodeId(pluginId, nodeName)) is not { } loadedNodeInfo)
        {
            throw new InvalidOperationException($"Unable to get node '{nodeName}' from plugin '{pluginName}'");
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
            Info = loadedNodeInfo
        };
    }

    public IWrappedNode FromNodeInfo(ILoadedNodeInfo loadedNode, ILoadedNodeManager loadedNodeManager)
    {
        ArgumentNullException.ThrowIfNull(loadedNode.NodeType.FullName);
        var persistentDictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
        persistentDictionary[PluginKey].GetValueOrInitialize(loadedNode.PluginId.Name);
        persistentDictionary[PluginVersionKey].GetValueOrInitialize(loadedNode.PluginId.Version);
        persistentDictionary[NodeNameKey].GetValueOrInitialize(loadedNode.NodeType.FullName);
        return FromPersistentData(persistentDictionary, loadedNodeManager);
    }
}
