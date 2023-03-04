using System;
using Laminar.Contracts.Primitives;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.Domain.ValueObjects;
using Laminar.Implementation.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.UserInterface;

namespace Laminar.Implementation.Scripting.Nodes;

public class WrappedNode<T> : IWrappedNode where T : INode, new()
{
    readonly T _coreNode;
    readonly INodeFactory _factory;
    Action? _preEvaluateAction;

    public WrappedNode(INodeRow nameRow, INodeFactory nodeFactory, T node, INotifyCollectionChangedHelper collectionHelper)
    {
        _factory = nodeFactory;
        _coreNode = node;
        Rows = new FlattenedObservableTree<INodeRow>(node.Components);
        collectionHelper.HelperInstance(Rows).ItemAdded += Rows_ItemAdded;
        collectionHelper.HelperInstance(Rows).ItemRemoved += Rows_ItemRemoved;

        NameRow = nameRow;

        foreach (var row in Rows)
        {
            RegisterRow(row);
        }
    }

    public INotificationClient<LaminarExecutionContext>? UserChangedValueNotificationClient { get; set; }

    public INodeRow NameRow { get; }

    public IReadOnlyObservableCollection<INodeRow> Rows { get; set; }

    public ObservableValue<Point> Location { get; set; } = new ObservableValue<Point>(new Point { X = 0, Y = 0 });

    public Identifier<IWrappedNode> Id { get; } = Identifier<IWrappedNode>.New();

    public IWrappedNode Clone(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        return _factory.CloneNode<T>(this, userChangedValueNotificationClient);
    }

    public void TriggerNotification(LaminarExecutionContext context)
    {
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
        if (_preEvaluateAction is not null)
        {
            _preEvaluateAction();
        }

        _coreNode.Evaluate();

        if (context.ExecutionFlags.HasUIUpdateFlag())
        {
            foreach (INodeRow field in Rows)
            {
                if (field.CentralDisplay is IRefreshable refreshable)
                {
                    refreshable.Refresh();
                }
            }
        }
    }

    private void Rows_ItemRemoved(object? sender, ItemRemovedEventArgs<INodeRow> e)
    {
        if (e.Item.OutputConnector?.PreEvaluateAction is Action currentActionO)
        {
            _preEvaluateAction -= currentActionO;
        }

        if (e.Item.InputConnector?.PreEvaluateAction is Action currentActionI)
        {
            _preEvaluateAction -= currentActionI;
        }

        e.Item.StartExecution -= Row_StartExecution;
    }

    private void Rows_ItemAdded(object? sender, ItemAddedEventArgs<INodeRow> e)
    {
        RegisterRow(e.Item);
    }

    private void RegisterRow(INodeRow row)
    {
        if (row.OutputConnector?.PreEvaluateAction is Action currentActionO)
        {
            _preEvaluateAction += currentActionO;
        }

        if (row.InputConnector?.PreEvaluateAction is Action currentActionI)
        {
            _preEvaluateAction += currentActionI;
        }

        row.StartExecution += Row_StartExecution;
    }

    private void Row_StartExecution(object? sender, LaminarExecutionContext e) => TriggerNotification(e);
}
