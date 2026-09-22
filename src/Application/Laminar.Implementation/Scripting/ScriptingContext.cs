using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Observables;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting;

internal sealed class ScriptingContext : IScriptingContext
{
    private readonly IScriptExecutionInstance _executionInstance; 
    private readonly IDisposable _connectionsSubscription;
    private readonly IWritableNodeGraph _nodeGraph;

    public ScriptingContext(IWritableNodeGraph nodeGraph, IExecutionManager executionManager)
    {
        _nodeGraph = nodeGraph;
        _executionInstance = executionManager.CreateExecutionInstance(nodeGraph);
        _connectionsSubscription = new CompositeDisposable(
            nodeGraph.Connections.SubscribeForEach(OnConnectionAdded, OnConnectionRemoved),
            nodeGraph.Nodes.SubscribeForEach(OnNodeAdded, OnNodeRemoved));
    }

    public required IScript HostScript { get; init; }

    public INotificationClient<LaminarExecutionContext> UserChangedValueNotificationClient => _executionInstance;

    public INodeCollection Nodes => _nodeGraph;

    public INodeGraph NodeGraph => _nodeGraph;
    
    public void Dispose()
    {
        _connectionsSubscription.Dispose();
        _executionInstance.Dispose();
        _nodeGraph.Dispose();
    }
    
    private static void OnConnectionRemoved(IConnection connection)
    {
        connection.InputConnector.OnDisconnectedFrom(connection.OutputConnector);
        connection.OutputConnector.OnDisconnectedFrom(connection.InputConnector);
    }

    private static void OnConnectionAdded(IConnection connection)
    {
        connection.InputConnector.OnConnectedTo(connection.OutputConnector);
        connection.OutputConnector.OnConnectedTo(connection.InputConnector);
    }

    private void OnNodeAdded(INodeContainer node) => ((NodeContainer)node).AttachTo(this);

    private static void OnNodeRemoved(INodeContainer node) => ((NodeContainer)node).DetachFromHost();
}