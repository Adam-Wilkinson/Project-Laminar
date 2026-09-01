using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Observables;
using Laminar.Domain.Observables.Collections;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeWrapping;

internal sealed class RuntimeNodeInstance : IRuntimeNodeInstance
{
    private readonly IDisposable _rowsChangedSubscription;
    private readonly INodeRow _headerRow;
    private readonly IReadOnlyObservableCollection<INodeRow> _rows;
    private Action? _preEvaluateAction;
    
    public RuntimeNodeInstance(
        INode node,
        INodeRow header,
        IReadOnlyObservableCollection<INodeRow> rows,
        INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        CoreNode = node;
        UserChangedValueNotificationClient = userChangedValueNotificationClient;
        _headerRow = header;
        _rows = rows;
        RowAdded(header);
        _rowsChangedSubscription = rows.SubscribeForEach(RowAdded, RowRemoved);
    }

    public INotificationClient<LaminarExecutionContext>? UserChangedValueNotificationClient { get; set; }

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
            context = context with { ExecutionSource = this };
        }

        if (UserChangedValueNotificationClient is null)
        {
            Update(context);
        }
        else
        {
            UserChangedValueNotificationClient.TriggerNotification(context);
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