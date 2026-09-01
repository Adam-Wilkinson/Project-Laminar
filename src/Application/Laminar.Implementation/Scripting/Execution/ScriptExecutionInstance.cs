using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Observables.Collections;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionInstance : IScriptExecutionInstance
{
    private readonly INodeTree _nodeTree;
    private readonly IExecutionOrderFinder _orderFinder;
    private readonly IExceptionHandler _exceptionHandler;
    private readonly CompositeDisposable _nodeTreeSubscriptions;
    
    private bool _isDisposed;

    public ScriptExecutionInstance(INodeTree nodeTree, IExecutionOrderFinder orderFinder, IExceptionHandler exceptionHandler)
    {
        _nodeTree = nodeTree;
        _orderFinder = orderFinder;
        _exceptionHandler = exceptionHandler;

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
            if (iter[0].Execute(context).Exception is not { } exception) return;
            _exceptionHandler.OnException(exception);
            return;
        }
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < iter.Length; i++)
        {
            if (iter[i].Execute(context).Exception is not { } exception) continue;
            _exceptionHandler.OnException(exception);
            break;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _nodeTreeSubscriptions.Dispose();
    }

    private static void OnNodeRemoved(INodeContainer nodeContainer) => nodeContainer.RuntimeNode?.UserChangedValueNotificationClient = null;

    private void OnNodeAdded(INodeContainer nodeContainer)
    {
        if (nodeContainer.RuntimeNode?.UserChangedValueNotificationClient is not null)
        {
            throw new InvalidOperationException($"The node {nodeContainer} appears to already have an execution instance. Changing the instance without proper disposable will likely result in an error");
        }
        
        nodeContainer.RuntimeNode?.UserChangedValueNotificationClient = this;
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
