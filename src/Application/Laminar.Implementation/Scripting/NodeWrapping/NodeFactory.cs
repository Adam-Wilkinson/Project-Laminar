using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Base.UserInterface;
using Laminar.Implementation.Scripting.Notifications;
using Laminar.PluginFramework;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class NodeFactory(IEncodableDataFactory dataFactory, IExceptionHandler exceptionHandler) : INodeFactory
{
    private const string PluginKey = "plugin";
    private const string PluginVersionKey = "plugin-version";
    private const string NodeNameKey = "type";
    
    private readonly NodeNotificationFactory _notificationFactory = new();
    
    public INodeContainer FromPersistentData(IPersistentDictionary persistentDictionary, IRuntimeHost host)
    {
        var pluginName = persistentDictionary[PluginKey].GetValue<string>().Value;
        var pluginVersion = persistentDictionary[PluginVersionKey].GetValue<SemanticVersion>().Value;
        var nodeName = persistentDictionary[NodeNameKey].GetValue<string>().Value;
        var nodeDescriptor = new NodeDescriptor(new VersionedPluginId(pluginName, pluginVersion), nodeName);
        
        var nameRow = LaminarFactory.Component.CreateSingleRow(null, new ObservableValueInterfaceData<EditableLabel, string>(persistentDictionary["Name"].GetValueOrInitialize(""))
        {
            Name = "",
            Definition = new EditableLabel()
        }, null);

        return new NodeContainer(nodeDescriptor, nameRow, persistentDictionary, _notificationFactory, host.PluginManager, exceptionHandler);
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
