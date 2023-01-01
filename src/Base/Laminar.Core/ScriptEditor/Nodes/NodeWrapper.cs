using System;
using System.Collections.ObjectModel;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.Primitives;
using Laminar.Domain.ValueObjects;
using Laminar.PluginFramework.NodeSystem;
using Laminar_PluginFramework.NodeSystem.Nodes;

namespace Laminar.Core.ScriptEditor.Nodes;

public class NodeWrapper<T> : INodeWrapper where T : INode, new()
{
    readonly T _coreNode;
    readonly INotificationClient<LaminarExecutionContext>? _userChangedValueNotificationClient;
    readonly INodeFactory _factory;

    Action? _preEvaluateAction;

    public NodeWrapper(INodeRowWrapper nameRow, INodeRowCollectionFactory rowCollectionFactory, INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient, INodeFactory nodeFactory, T node)
    {
        _factory = nodeFactory;
        _coreNode = node;
        Fields = rowCollectionFactory.CreateNodeRowsForObject(_coreNode, this);
        _userChangedValueNotificationClient = userChangedValueNotificationClient;

        NameRow = nameRow;

        foreach (var field in Fields)
        {
            if (field.OutputConnector is not null && field.OutputConnector.NodeIOConnector.PreEvaluateAction is Action currentActionO)
            {
                _preEvaluateAction += currentActionO;
            }

            if (field.InputConnector?.NodeIOConnector.PreEvaluateAction is Action currentActionI)
            {
                _preEvaluateAction += currentActionI;
            }
        }
    }

    public INodeRowWrapper NameRow { get; }

    public ObservableCollection<INodeRowWrapper> Fields { get; set; }

    public ObservableValue<Point> Location { get; set; } = new ObservableValue<Point>(new Point { X = 0, Y = 0 });

    public Identifier<INodeWrapper> Id { get; } = Identifier<INodeWrapper>.New();

    public INodeWrapper Clone(INotificationClient<LaminarExecutionContext>? userChangedValueNotificationClient)
    {
        return _factory.CloneNode<T>(this, userChangedValueNotificationClient);
    }

    public void TriggerNotification(LaminarExecutionContext context)
    {
        if (_userChangedValueNotificationClient is null)
        {
            Update(context);
        }
        else
        {
            _userChangedValueNotificationClient.TriggerNotification(context);
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
            foreach (INodeRowWrapper field in Fields)
            {
                field.RefreshDisplay();
            }
        }
    }
}
