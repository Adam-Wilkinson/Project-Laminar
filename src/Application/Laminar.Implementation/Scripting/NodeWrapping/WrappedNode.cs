using System;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.NodeWrapping;

public class WrappedNode<T> : IWrappedNode where T : INode, new()
{
    private readonly T _coreNode;
    private readonly INodeFactory _factory;

    private Action? _preEvaluateAction;

    public WrappedNode(INodeRow nameRow, INodeFactory nodeFactory, T node)
    {
        _factory = nodeFactory;
        _coreNode = node;
        NameRow = nameRow;

        Rows = new FlattenedObservableTree<INodeRow>(node.Components);
        Rows.HelperInstance().ItemAdded += Rows_ItemAdded;
        Rows.HelperInstance().ItemRemoved += Rows_ItemRemoved;

        foreach (var row in Rows)
        {
            RegisterRow(row);
        }
    }

    public INotificationClient<LaminarExecutionContext>? UserChangedValueNotificationClient { get; set; }

    public INodeRow NameRow { get; }

    public IReadOnlyObservableCollection<INodeRow> Rows { get; set; }

    public ObservableValue<Point> Location { get; set; } = new(new Point { X = 0, Y = 0 });

    public Identifier<IWrappedNode> Id { get; } = Identifier<IWrappedNode>.New();

    public IWrappedNode Clone(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        return _factory.CloneNode<T>(this, userChangedValueNotificationClient);
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

        _coreNode.Evaluate();

        if (!context.ExecutionFlags.IsUiUpdate) return;
        
        foreach (var field in Rows)
        {
            if (field.CentralDisplay is IRefreshable refreshable)
            {
                refreshable.Refresh();
            }
        }
    }

    private void Rows_ItemRemoved(object? sender, ItemRemovedEventArgs<INodeRow> e)
    {
        if (e.Item.OutputConnector?.PreEvaluateAction is { } outputPreevaluate)
        {
            _preEvaluateAction -= outputPreevaluate;
        }

        if (e.Item.InputConnector?.PreEvaluateAction is { } inputPreevaluate)
        {
            _preEvaluateAction -= inputPreevaluate;
        }

        e.Item.StartExecution -= Row_StartExecution;
    }

    private void Rows_ItemAdded(object? sender, ItemAddedEventArgs<INodeRow> e)
    {
        RegisterRow(e.Item);
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
}
