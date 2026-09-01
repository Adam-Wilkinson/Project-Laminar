using Laminar.Contracts.Base.PluginLoading;
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

    public INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IRuntimeHost host)
    {
        var pluginName = persistentDictionary[PluginKey].GetValue<string>().Value;
        var pluginVersion = persistentDictionary[PluginVersionKey].GetValue<SemanticVersion>().Value;
        var nodeName = persistentDictionary[NodeNameKey].GetValue<string>().Value;
        var nodeDescriptor = new NodeDescriptor(new VersionedPluginId(pluginName, pluginVersion), nodeName);
        
        var nameRow = LaminarFactory.Component.CreateSingleRow(null, new ObservableValueInterfaceData<EditableLabel, string>(persistentDictionary["Name"].GetValueOrInitialize(nodeName))
        {
            Name = "",
            Definition = new EditableLabel()
        }, null);

        return new NodeContainer(nodeDescriptor, nameRow, persistentDictionary, host.PluginManager);
    }

    public INodeContainer FromDescriptor(NodeDescriptor descriptor, IRuntimeHost host)
    {
        var persistentDictionary = dataFactory.GetEncodableData<IPersistentDictionary>();
        persistentDictionary[PluginKey].GetValueOrInitialize(descriptor.Plugin.Name);
        persistentDictionary[PluginVersionKey].GetValueOrInitialize(descriptor.Plugin.Version);
        persistentDictionary[NodeNameKey].GetValueOrInitialize(descriptor.NodeName);
        return FromPersistentData(persistentDictionary, host);
    }
}
