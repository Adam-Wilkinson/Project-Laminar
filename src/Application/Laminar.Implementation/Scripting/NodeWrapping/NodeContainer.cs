using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Exceptions;
using Laminar.Domain.Notifications;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.Observables.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Notifications;
using Laminar.Implementation.Storage.PersistentData;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal sealed class NodeContainer : INodeContainer
{
    private readonly IPersistentDictionary _persistentDictionary;
    private readonly BoundObservableCollection<INodeRow> _rows = new();
    private readonly IExceptionHandler _exceptionHandler;
    
    private IDisposable? _persistentRowsSynchronizer;
    
    public NodeContainer(
        NodeDescriptor descriptor,
        INodeRow<IInterfaceData<EditableLabel, string>> nameRow, 
        IPersistentDictionary persistentDictionary,
        NodeNotificationFactory notificationFactory,
        IPluginManager pluginManager,
        IExceptionHandler exceptionHandler)
    {
        _persistentDictionary = persistentDictionary;
        _exceptionHandler = exceptionHandler;
        
        Descriptor = descriptor;
        IsCollapsed = persistentDictionary[nameof(IsCollapsed)].GetValueOrInitialize(false);
        Location = persistentDictionary[nameof(Location)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        NameRow = nameRow;

        if (!pluginManager.TryGetInstalledPlugin(Descriptor.Plugin, out var plugin))
        {
            Notifications.AddNotification(notificationFactory.PluginNotInstalled(Descriptor.Plugin, this, pluginManager));
            
            _rows.BindTo(new ObservableCollectionImpl<INodeRow>([
                .. persistentDictionary[nameof(Rows)].GetOrCreateCollection<IPersistentList>()
                    .Select(_ => new StubNodeRow())
            ]));
            return;
        }

        if (!plugin.TryGetNodeInfo(Descriptor.NodeName, out var nodeInfo))
        {
            Notifications.AddNotification(notificationFactory.PluginDoesNotContainNode(Descriptor.Plugin, this));
            
            _rows.BindTo(new ObservableCollectionImpl<INodeRow>([
                .. persistentDictionary[nameof(Rows)].GetOrCreateCollection<IPersistentList>()
                    .Select(_ => new StubNodeRow())
            ]));
            return;
        }

        OnNodeInfoLoaded(nodeInfo);
    }

    internal void AttachTo(INodeHost host)
    {
        if (Host is not null) throw new InvalidOperationException("This node is already attached to a host");
        Host = host;
    }

    internal void DetachFromHost()
    {
        Host = null;
    }

    internal void OnNodeInfoLoaded(ILoadedNodeInfo nodeInfo)
    {
        if (RuntimeNode is not null) throw new InvalidOperationException("This node already has a runtime implementation");

        try
        {
            var node = nodeInfo.CreateInstance();
            if (string.IsNullOrWhiteSpace(NameRow.CentralDisplay.Value))
            {
                NameRow.CentralDisplay.Value = node.NodeName;
            }

            _rows.BindTo(new FlattenedObservableTree<INodeRow>(node.Components));

            _persistentRowsSynchronizer = _persistentDictionary[nameof(Rows)]
                .GetOrCreateCollection<IPersistentList>()
                .InitializeAndSyncTo(Rows,
                    new PersistentValueAdapter<INodeRow>(row => row?.GetType() ?? typeof(INodeRow))
                    {
                        Mode = PersistenceAdapterMode.Hydrate
                    });

            RuntimeNode = new RuntimeNodeInstance(node, NameRow, Rows, null);
        }
        catch (Exception ex)
        {
            _exceptionHandler.OnException(new ErrorCreatingNodeException(nodeInfo.NodeType.ToString(), ex));
            Notifications.AddNotification(new ErrorCreatingNodeNotification(nodeInfo.NodeType.ToString()));
        }
    }
    
    public INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }

    public IReadOnlyObservableCollection<INodeRow> Rows => _rows;

    public IObservableValue<bool> IsCollapsed { get; }

    public IObservableValue<Point> Location { get; }
    
    public NodeDescriptor Descriptor { get; }

    public IEncodableData PersistentData => _persistentDictionary;

    public NotificationManager Notifications { get; } = new();
    
    public IRuntimeNodeInstance? RuntimeNode { get; private set; }
    
    internal INodeHost? Host { get; private set; }

    public override string ToString() => $"{NameRow.CentralDisplay.Value} ({RuntimeNode?.CoreNode})";

    public void Dispose()
    {
        _persistentRowsSynchronizer?.Dispose();
        RuntimeNode?.Dispose();
    }
}
