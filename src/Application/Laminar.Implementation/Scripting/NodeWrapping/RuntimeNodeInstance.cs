using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Observables.Collections;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal sealed class RuntimeNodeInstance : IRuntimeNodeInstance, IDisposable
{
    private readonly IDisposable _rowsChangedSubscription;
    private readonly INodeRow _headerRow;
    private readonly IReadOnlyObservableCollection<INodeRow> _rows;
    private readonly NodeContainer _container;
    private Action? _preEvaluateAction;
    
    public RuntimeNodeInstance(
        NodeContainer container,
        INode node,
        INodeRow header,
        IReadOnlyObservableCollection<INodeRow> rows)
    {
        _container = container;
        CoreNode = node;
        _headerRow = header;
        _rows = rows;
        RowAdded(header);
        _rowsChangedSubscription = rows.SubscribeForEach(RowAdded, RowRemoved);
    }
    
    public INode CoreNode { get; }
    
    private void RowAdded(INodeRow row)
    {
        if (row.OutputConnector?.PreEvaluateAction is { } outputPreevaluate)
        {
            _preEvaluateAction += outputPreevaluate;
        }

        if (row.InputConnector?.PreEvaluateAction is { } inputPreevaluate)
        {
            _preEvaluateAction += inputPreevaluate;
        }

        row.StartExecution += Row_StartExecution;
    }

    private void RowRemoved(INodeRow row)
    {
        if (row.OutputConnector?.PreEvaluateAction is { } outputPreevaluate)
        {
            _preEvaluateAction -= outputPreevaluate;
        }

        if (row.InputConnector?.PreEvaluateAction is { } inputPreevaluate)
        {
            _preEvaluateAction -= inputPreevaluate;
        }

        row.StartExecution -= Row_StartExecution;
    }

    private void Row_StartExecution(object? sender, LaminarExecutionContext e)
    {
        TriggerNotification(e);
    }
    
    public void TriggerNotification(LaminarExecutionContext context)
    {
        if (context.ExecutionSource is null)
        {
            context = context with { ExecutionSource = _container };
        }

        if (_container.Host?.UserChangedValueNotificationClient is not { } notificationClient)
        {
            Update(context);
        }
        else
        {
            notificationClient.TriggerNotification(context);
        }
    }

    public void Update(LaminarExecutionContext context)
    {
        _preEvaluateAction?.Invoke();

        CoreNode.Evaluate();

        if (!context.ExecutionFlags.IsUiUpdate) return;
        
        foreach (var field in _rows)
        {
            if (field.CentralDisplay is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }
    }

    public void Dispose()
    {
        _rowsChangedSubscription.Dispose();
        RowRemoved(_headerRow);
    }
}