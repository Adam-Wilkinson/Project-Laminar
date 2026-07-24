using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript, IDisposable
{
    private const string NodeTreeKey = "NodeTree";
    
    private readonly IDisposable _connectionsChangedSubscription;
    private readonly IScriptExecutionInstance _executionInstance;
    
    public Script(IPersistentDictionary persistentData, IScriptExecutionManager executionManager, IScriptingFactory scriptingFactory)
    {
        NodeTree = scriptingFactory.NodeTreeFromPersistentData(persistentData[NodeTreeKey]
            .GetOrCreateCollection<IPersistentDictionary>());

        _executionInstance = executionManager.CreateExecutionInstance(NodeTree);
        
        _connectionsChangedSubscription = NodeTree.Connections.SubscribeForEach(OnConnectionAdded, OnConnectionRemoved);
        
        Pan = persistentData[nameof(Pan)].GetValueOrInitialize(new Point { X = 0, Y = 0 });
        Zoom = persistentData[nameof(Zoom)].GetValueOrInitialize(1.0);
        Data = persistentData;
    }

    private void OnConnectionAdded(IConnection connection)
    {
        connection.InputConnector.OnConnectedTo(connection.OutputConnector);
        connection.OutputConnector.OnConnectedTo(connection.InputConnector);
    }

    private void OnConnectionRemoved(IConnection connection)
    {
        connection.InputConnector.OnDisconnectedFrom(connection.OutputConnector);
        connection.OutputConnector.OnDisconnectedFrom(connection.InputConnector);
    }
    
    public INodeTree NodeTree { get; }
    
    public IObservableValue<Point> Pan { get; }

    public IObservableValue<double> Zoom { get; }

    public IPersistentDictionary Data { get; }

    public void Dispose()
    {
        NodeTree.Dispose();
        _connectionsChangedSubscription.Dispose();
    }
}
