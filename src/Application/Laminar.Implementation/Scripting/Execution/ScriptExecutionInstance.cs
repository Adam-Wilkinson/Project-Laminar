using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionInstance : IScriptExecutionInstance
{
    private readonly INodeTree _nodeTree;
    private readonly IExecutionOrderFinder _orderFinder;
    private readonly CompositeDisposable _nodeTreeSubscriptions;
    
    private bool _isDisposed;

    public ScriptExecutionInstance(INodeTree nodeTree, IExecutionOrderFinder orderFinder)
    {
        _nodeTree = nodeTree;
        _orderFinder = orderFinder;

        _nodeTreeSubscriptions =
            new CompositeDisposable(
                _nodeTree.Connections.SubscribeForEach(OnConnectionAdded, OnConnectionRemoved),
                _nodeTree.Nodes.SubscribeForEach(OnNodeAdded, OnNodeRemoved));
    }

    public ScriptState State { get; private set; } = ScriptState.Active;

    public bool IsShownInUI { get; set; } = true;

    public void TriggerNotification(LaminarExecutionContext context)
    {
        if (_isDisposed) return;
        
        State = ScriptState.Running;

        if (IsShownInUI)
        {
            context = context with { ExecutionFlags = context.ExecutionFlags | UiUpdateExecutionFlag.Value };
        }

        ReadOnlySpan<IConditionalExecutionBranch> iter = new(_orderFinder.GetExecutionBranchesFrom(context, _nodeTree));

        if (iter.Length == 1)
        {
            iter[0].Execute(context);
        }
        else
        {
            for (int i = 0; i < iter.Length; i++)
            {
                iter[i].Execute(context);
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _nodeTreeSubscriptions.Dispose();
    }

    private static void OnNodeRemoved(IWrappedNode node) => node.UserChangedValueNotificationClient = null;

    private void OnNodeAdded(IWrappedNode node)
    {
        if (node.UserChangedValueNotificationClient is not null)
        {
            throw new InvalidOperationException($"The node {node} appears to already have an execution instance. Changing the instance without proper disposable will likely result in an error");
        }
        
        node.UserChangedValueNotificationClient = this;
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
}
