using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Contracts.Storage.PersistentData;
using Laminar.Domain.Notification;
using Laminar.Domain.Notification.Collections;
using Laminar.Domain.Notification.Value;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;
using Laminar.PluginFramework.UserInterface.UserInterfaceDefinitions;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public sealed class WrappedNode : IWrappedNode, IDisposable
{
    private readonly IDisposable _rowsChangedSubscription;
    private Action? _preEvaluateAction;
    
    public WrappedNode(INodeRow<IInterfaceData<EditableLabel, string>> nameRow, INode node, IPersistentDictionary persistentDictionary)
    {
        CoreNode = node;
        NameRow = nameRow;

        Rows = new FlattenedObservableTree<INodeRow>(node.Components);
        _rowsChangedSubscription = Rows.SubscribeForEach(RegisterRow, RowRemoved);
        
        Id = persistentDictionary[nameof(Id)].GetValueOrDefault(GuidIdentifier<IWrappedNode>.New()).Value;
        IsCollapsed = persistentDictionary[nameof(IsCollapsed)].GetValueOrDefault(false);
        Location = persistentDictionary[nameof(Location)].GetValueOrDefault(new Point {X = 0, Y = 0});
        
        foreach (var row in Rows)
        {
            RegisterRow(row);
        }
    }

    public INotificationClient<LaminarExecutionContext>? UserChangedValueNotificationClient { get; set; }

    public INode CoreNode { get; }

    public INodeRow<IInterfaceData<EditableLabel, string>> NameRow { get; }
    
    public IReadOnlyObservableCollection<INodeRow> Rows { get; set; }

    public IObservableValue<bool> IsCollapsed { get; }

    public IObservableValue<Point> Location { get; }

    public GuidIdentifier<IWrappedNode> Id { get; }

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
        
        foreach (var field in Rows)
        {
            if (field.CentralDisplay is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }
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

    private void RegisterRow(INodeRow row)
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

    private void Row_StartExecution(object? sender, LaminarExecutionContext e)
    {
        TriggerNotification(e);
    }

    public override string ToString() => $"{NameRow.CentralDisplay.Value} ({CoreNode})";

    public void Dispose()
    {
        _rowsChangedSubscription.Dispose();
    }
}
