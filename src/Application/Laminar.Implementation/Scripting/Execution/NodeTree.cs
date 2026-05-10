using System;
using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

public class NodeTree : INodeTree
{
    private readonly Dictionary<IOutputConnector, List<IWrappedNode>> _connections = new();
    private readonly Dictionary<IIOConnector, IWrappedNode> _connectorParents = new();
    private readonly Dictionary<IWrappedNode, EventHandler<ItemAddedEventArgs<INodeRow>>> _nodeRowAddedDelegates = new();
    private readonly Dictionary<IWrappedNode, EventHandler<ItemRemovedEventArgs<INodeRow>>> _nodeRowRemovedDelegates = new();

    public event EventHandler? Changed;

    public NodeTree(IScript script)
    { 
        script.Nodes.HelperInstance().ItemAdded += NodeAdded;
        script.Nodes.HelperInstance().ItemRemoved += NodeRemoved;

        script.Connections.HelperInstance().ItemAdded += ConnectionAdded;
        script.Connections.HelperInstance().ItemRemoved += ConnectionRemoved;
    }

    public IWrappedNode GetParentNode(IIOConnector connector) => _connectorParents[connector];

    public IReadOnlyList<IWrappedNode> GetConnections(IOutputConnector outputConnector) => _connections.TryGetValue(outputConnector, out var connections) ? connections : Array.Empty<IWrappedNode>();

    private void NodeRemoved(object? sender, ItemRemovedEventArgs<IWrappedNode> e)
    {
        e.Item.Rows.HelperInstance().ItemAdded -= _nodeRowAddedDelegates[e.Item];
        _nodeRowAddedDelegates.Remove(e.Item);
        e.Item.Rows.HelperInstance().ItemRemoved -= _nodeRowRemovedDelegates[e.Item];
        _nodeRowRemovedDelegates.Remove(e.Item);

        foreach (INodeRow row in e.Item.Rows)
        {
            RowRemoved(e.Item, row);
        }
    }

    private void NodeAdded(object? sender, ItemAddedEventArgs<IWrappedNode> e)
    {
        _nodeRowAddedDelegates[e.Item] = (_, newRow) => RowAdded(e.Item, newRow.Item);
        e.Item.Rows.HelperInstance().ItemAdded += _nodeRowAddedDelegates[e.Item];
        _nodeRowRemovedDelegates[e.Item] = (_, removedRow) => RowRemoved(e.Item, removedRow.Item);
        e.Item.Rows.HelperInstance().ItemRemoved += _nodeRowRemovedDelegates[e.Item];

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
