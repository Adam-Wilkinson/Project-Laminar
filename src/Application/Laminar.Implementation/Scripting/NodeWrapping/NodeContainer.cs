using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notifications;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal sealed class NodeContainer : INodeContainer
{
    private readonly IDisposable? _persistentRowsSynchronizer;
    private readonly IPersistentDictionary _persistentDictionary;
    private readonly BoundObservableCollection<INodeRow> _rows = new();
    
    public NodeContainer(
        NodeDescriptor descriptor,
        INodeRow<IInterfaceData<EditableLabel, string>> nameRow, 
        IPersistentDictionary persistentDictionary,
        IPluginManager pluginManager)
    {
        _persistentDictionary = persistentDictionary;
        
        Descriptor = descriptor;
        IsCollapsed = persistentDictionary[nameof(IsCollapsed)].GetValueOrInitialize(false);
        Location = persistentDictionary[nameof(Location)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        NameRow = nameRow;

        if (!pluginManager.TryGetInstalledPlugin(Descriptor.Plugin, out var plugin))
        {
            Notifications.AddNotification(new INodeContainer.PluginMissingNotificationTemplate(Descriptor.Plugin));
            _rows.BindTo(new ObservableCollectionImpl<INodeRow>([
                .. persistentDictionary[nameof(Rows)].GetOrCreateCollection<IPersistentList>()
                    .Select(_ => new StubNodeRow())
            ]));
            return;
        }

        if (!plugin.TryGetNodeInfo(Descriptor.NodeName, out var nodeInfo))
        {
            Notifications.AddNotification(new INodeContainer.PluginDoesNotContainNodeNotificationTemplate(Descriptor.Plugin, Descriptor.NodeName));
            _rows.BindTo(new ObservableCollectionImpl<INodeRow>([
                .. persistentDictionary[nameof(Rows)].GetOrCreateCollection<IPersistentList>()
                    .Select(_ => new StubNodeRow())
            ]));
            return;
        }

        var newNode = nodeInfo.CreateInstance();
        _rows.BindTo(new FlattenedObservableTree<INodeRow>(newNode.Components));
            
        _persistentRowsSynchronizer = persistentDictionary[nameof(Rows)]
            .GetOrCreateCollection<IPersistentList>()
            .InitializeAndSyncTo(Rows, new PersistentValueAdapter<INodeRow>(row => row?.GetType() ?? typeof(INodeRow))
            {
                Mode = PersistenceAdapterMode.Hydrate
            });
            
        RuntimeNode = new RuntimeNodeInstance(newNode, nameRow, Rows, null);
    }
    
    public INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }

    public IReadOnlyObservableCollection<INodeRow> Rows => _rows;

    public IObservableValue<bool> IsCollapsed { get; }

    public IObservableValue<Point> Location { get; }
    
    public NodeDescriptor Descriptor { get; }

    public IEncodableData PersistentData => _persistentDictionary;

    public NotificationManager Notifications { get; } = new();
    
    public IRuntimeNodeInstance? RuntimeNode { get; }

    public override string ToString() => $"{NameRow.CentralDisplay.Value} ({RuntimeNode?.CoreNode})";

    public void Dispose()
    {
        _persistentRowsSynchronizer?.Dispose();
        RuntimeNode?.Dispose();
    }
}
