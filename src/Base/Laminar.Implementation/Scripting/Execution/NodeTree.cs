using System;
using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Contracts.Components;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

public class NodeTree : INodeTree
{
    readonly INotifyCollectionChangedHelper _notifyHelper;

    readonly Dictionary<IOutputConnector, List<IWrappedNode>> _connections = new();
    readonly Dictionary<IIOConnector, IWrappedNode> _connectorParents = new();
    readonly Dictionary<IWrappedNode, EventHandler<ItemAddedEventArgs<INodeRow>>> _nodeRowAddedDelegates = new();
    readonly Dictionary<IWrappedNode, EventHandler<ItemRemovedEventArgs<INodeRow>>> _nodeRowRemovedDelegates = new();

    public event EventHandler? Changed;

    public NodeTree(IScript script, INotifyCollectionChangedHelper notificationHelper)
    {
        _notifyHelper = notificationHelper;

        notificationHelper.HelperInstance(script.Nodes).ItemAdded += NodeAdded;
        notificationHelper.HelperInstance(script.Nodes).ItemRemoved += NodeRemoved;

        notificationHelper.HelperInstance(script.Connections).ItemAdded += ConnectionAdded;
        notificationHelper.HelperInstance(script.Connections).ItemRemoved += ConnectionRemoved;
    }

    public IWrappedNode GetParentNode(IIOConnector connector) => _connectorParents[connector];

    public IReadOnlyList<IWrappedNode> GetConnections(IOutputConnector outputConnector) => _connections.TryGetValue(outputConnector, out var connections) ? connections : Array.Empty<IWrappedNode>();

    private void NodeRemoved(object? sender, ItemRemovedEventArgs<IWrappedNode> e)
    {
        _notifyHelper.HelperInstance(e.Item.Rows).ItemAdded -= _nodeRowAddedDelegates[e.Item];
        _nodeRowAddedDelegates.Remove(e.Item);
        _notifyHelper.HelperInstance(e.Item.Rows).ItemRemoved -= _nodeRowRemovedDelegates[e.Item];
        _nodeRowRemovedDelegates.Remove(e.Item);

        foreach (INodeRow row in e.Item.Rows)
        {
            RowRemoved(e.Item, row);
        }
    }

    private void NodeAdded(object? sender, ItemAddedEventArgs<IWrappedNode> e)
    {
        _nodeRowAddedDelegates[e.Item] = (object? _, ItemAddedEventArgs<INodeRow> newRow) => RowAdded(e.Item, newRow.Item);
        _notifyHelper.HelperInstance(e.Item.Rows).ItemAdded += _nodeRowAddedDelegates[e.Item];
        _nodeRowRemovedDelegates[e.Item] = (object? _, ItemRemovedEventArgs<INodeRow> removedRow) => RowRemoved(e.Item, removedRow.Item);
        _notifyHelper.HelperInstance(e.Item.Rows).ItemRemoved += _nodeRowRemovedDelegates[e.Item];

        foreach (INodeRow row in e.Item.Rows)
        {
            RowAdded(e.Item, row);
        }
    }

    private void RowAdded(IWrappedNode node, INodeRow row)
    {
        if (row.InputConnector is not null)
        {
            _connectorParents[row.InputConnector] = node;
        }

        if (row.OutputConnector is not null)
        {
            _connectorParents[row.OutputConnector] = node;
        }
    }

    private void RowRemoved(IWrappedNode node, INodeRow row)
    {
        if (row.InputConnector is not null)
        {
            _connectorParents[row.InputConnector] = node;
        }

        if (row.OutputConnector is not null)
        {
            _connectorParents[row.OutputConnector] = node;
        }
    }

    private void ConnectionRemoved(object? sender, ItemRemovedEventArgs<IConnection> e)
    {
        IConnection removedConnection = e.Item;
        IWrappedNode outputNode = _connectorParents[removedConnection.OutputConnector];

        _connections[removedConnection.OutputConnector].Remove(_connectorParents[removedConnection.InputConnector]);

        if (_connections[removedConnection.OutputConnector].Count == 0)
        {
            _connections.Remove(removedConnection.OutputConnector);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private void ConnectionAdded(object? sender, ItemAddedEventArgs<IConnection> e)
    {
        IConnection newConnection = e.Item;
        IWrappedNode outputNode = _connectorParents[newConnection.OutputConnector];

        if (!_connections.ContainsKey(newConnection.OutputConnector))
        {
            _connections.Add(newConnection.OutputConnector, new List<IWrappedNode>());
        }

        _connections[newConnection.OutputConnector].Add(_connectorParents[newConnection.InputConnector]);

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
