using Laminar.Contracts.Base;
using Laminar.Contracts.Base.PluginLoading;
using Laminar.Contracts.Scripting;
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
    private readonly BoundObservableList<INodeRow> _rows = new();
    private readonly RuntimeNodeInstance? _runtimeNode;
    private readonly IDisposable? _persistentRowsSynchronizer;
    
    public NodeContainer(
        NodeDescriptor descriptor,
        INodeRow<IInterfaceData<EditableLabel, string>> nameRow, 
        IPersistentDictionary persistentDictionary,
        NodeNotificationFactory notificationFactory,
        IPluginManager pluginManager,
        IExceptionHandler exceptionHandler)
    {
        _persistentDictionary = persistentDictionary;

        Descriptor = descriptor;
        IsCollapsed = persistentDictionary[nameof(IsCollapsed)].GetValueOrInitialize(false);
        Location = persistentDictionary[nameof(Location)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        NameRow = nameRow;

        if (!pluginManager.TryGetInstalledPlugin(Descriptor.Plugin, out var plugin))
        {
            Notifications.AddNotification(notificationFactory.PluginNotInstalled(Descriptor.Plugin, this, pluginManager));

            _rows.BindTo(new ObservableList<INodeRow>(persistentDictionary[nameof(Rows)]
                .GetOrCreateCollection<IPersistentList>()
                .Select(_ => new StubNodeRow())));
            return;
        }

        if (!plugin.TryGetNodeInfo(Descriptor.NodeName, out var nodeInfo))
        {
            Notifications.AddNotification(notificationFactory.PluginDoesNotContainNode(Descriptor.Plugin, this));

            _rows.BindTo(new ObservableList<INodeRow>(persistentDictionary[nameof(Rows)]
                .GetOrCreateCollection<IPersistentList>()
                .Select(_ => new StubNodeRow())));
            return;
        }

        try
        {
            var node = nodeInfo.CreateInstance();
            if (string.IsNullOrWhiteSpace(NameRow.CentralDisplay.Value))
            {
                NameRow.CentralDisplay.Value = node.NodeName;
            }

            _rows.BindTo(new FlattenedObservableTree<INodeRow>(node.Components));
            
            var persistentRows = persistentDictionary[nameof(Rows)].GetOrCreateCollection<IPersistentList>();

            foreach (var row in persistentRows)
            {
                row.Reset(clearEncodedValue: false);
            }
            
            _persistentRowsSynchronizer = persistentRows.InitializeAndSyncTo(Rows,
                    new PersistentValueAdapter<INodeRow>(row => row?.GetType() ?? typeof(INodeRow))
                    {
                        Mode = PersistenceAdapterMode.Hydrate
                    });

            _runtimeNode = new RuntimeNodeInstance(this, node, NameRow, Rows);
        }
        catch (Exception ex)
        {
            exceptionHandler.OnException(new ErrorCreatingNodeException(nodeInfo.NodeType.ToString(), ex));
            Notifications.AddNotification(new ErrorCreatingNodeNotification(nodeInfo.NodeType.ToString()));
        }
    }

    internal void AttachTo(IScriptingContext host)
    {
        if (Host is not null) throw new InvalidOperationException("This node is already attached to a host");
        Host = host;
    }

    internal void DetachFromHost()
    {
        Host = null;
    }
    
    public INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }

    public IReadOnlyObservableList<INodeRow> Rows => _rows;

    public IObservableValue<bool> IsCollapsed { get; }

    public IObservableValue<Point> Location { get; }
    
    public NodeDescriptor Descriptor { get; }

    public IEncodableData PersistentData => _persistentDictionary;

    public NotificationManager Notifications { get; } = new();

    public IRuntimeNodeInstance? RuntimeNode => _runtimeNode;
    
    internal IScriptingContext? Host { get; private set; }

    public override string ToString() => $"{NameRow.CentralDisplay.Value} ({_runtimeNode?.CoreNode})";

    public void Dispose()
    {
        _persistentRowsSynchronizer?.Dispose();
        _runtimeNode?.Dispose();
    }
}
