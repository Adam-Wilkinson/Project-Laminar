using System;
using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Notification;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Execution;

public class NodeTree : INodeTree
{
    readonly ExecutionOrderFinder _executionOrderFinder = new();

    readonly Dictionary<IWrappedNode, List<IWrappedNode>> _dependentNodes = new();
    readonly Dictionary<IWrappedNode, IWrappedNode[]> _knownExecutionPaths = new();
    readonly Dictionary<IOutputConnector, List<IWrappedNode>> _connections = new();
    readonly Dictionary<(IIOConnector, int), ConditionalExecutionBranch[]> _executionPaths = new();
    readonly Dictionary<IIOConnector, IWrappedNode> _connectorParents = new();

    public NodeTree(IScript script, INotifyCollectionChangedHelper notificationHelper)
    {
        notificationHelper.HelperInstance(script.Nodes).ItemAdded += NodeAdded;
        notificationHelper.HelperInstance(script.Nodes).ItemRemoved += NodeRemoved;

        notificationHelper.HelperInstance(script.Connections).ItemAdded += ConnectionAdded;
        notificationHelper.HelperInstance(script.Connections).ItemRemoved += ConnectionRemoved;
    }

    public IConditionalExecutionBranch[] GetExecutionBranches(IIOConnector connector, ExecutionFlags flags)
    {
        //if (_executionPaths.TryGetValue(connector, out Dictionary<int, ConditionalExecutionBranch[]> connectorPaths) && connectorPaths.TryGetValue(flags.AsNumber, out ConditionalExecutionBranch[] branches))
        if (_executionPaths.TryGetValue((connector, flags.AsNumber), out ConditionalExecutionBranch[] branches))
        {
            return branches;
        }

        if (connector is IInputConnector && _connectorParents.ContainsKey(connector))
        {
            ConditionalExecutionBranch[] result = _executionOrderFinder.FindExecutionPath(_connectorParents[connector], flags, _connections);
            //AddExecutionPath(connector, flags.AsNumber, result);
            _executionPaths.Add((connector, flags.AsNumber), result);
            return result;
        }

        if (connector is IOutputConnector outputConnector)
        {
            ConditionalExecutionBranch[] result = _executionOrderFinder.FindExecutionPath(outputConnector, flags, _connections);
            //AddExecutionPath(connector, flags.AsNumber, result);
            _executionPaths.Add((connector, flags.AsNumber), result);
            return result;
        }

        return Array.Empty<IConditionalExecutionBranch>();
    }

    private void ConnectionRemoved(object? sender, ItemRemovedEventArgs<IConnection> e)
    {
        IConnection removedConnection = e.Item;
        IWrappedNode outputNode = _connectorParents[removedConnection.OutputConnector];
        _dependentNodes[outputNode].Remove(_connectorParents[removedConnection.InputConnector]);

        if (_dependentNodes[outputNode].Count == 0)
        {
            _dependentNodes.Remove(outputNode);
        }

        _connections[removedConnection.OutputConnector].Remove(_connectorParents[removedConnection.InputConnector]);

        if (_connections[removedConnection.OutputConnector].Count == 0)
        {
            _connections.Remove(removedConnection.OutputConnector);
        }

        _knownExecutionPaths.Clear();
    }

    private void ConnectionAdded(object? sender, ItemAddedEventArgs<IConnection> e)
    {
        IConnection newConnection = e.Item;
        IWrappedNode outputNode = _connectorParents[newConnection.OutputConnector];
        if (!_dependentNodes.ContainsKey(outputNode))
        {
            _dependentNodes.Add(outputNode, new List<IWrappedNode>());
        }

        _dependentNodes[outputNode].Add(_connectorParents[newConnection.InputConnector]);

        if (!_connections.ContainsKey(newConnection.OutputConnector))
        {
            _connections.Add(newConnection.OutputConnector, new List<IWrappedNode>());
        }

        _connections[newConnection.OutputConnector].Add(_connectorParents[newConnection.InputConnector]);

        _knownExecutionPaths.Clear();
    }

    private void NodeRemoved(object? sender, ItemRemovedEventArgs<IWrappedNode> e)
    {
        IWrappedNode removedNode = e.Item;
        foreach (IWrappedNodeRow row in removedNode.Rows)
        {
            if (row.InputConnector is not null)
            {
                _connectorParents.Remove(row.InputConnector.NodeIOConnector);
            }

            if (row.OutputConnector is not null)
            {
                _connectorParents.Remove(row.OutputConnector.NodeIOConnector);
            }
        }
    }

    private void NodeAdded(object? sender, ItemAddedEventArgs<IWrappedNode> e)
    {
        IWrappedNode newNode = e.Item;
        foreach (IWrappedNodeRow row in newNode.Rows)
        {
            if (row.InputConnector is not null)
            {
                _connectorParents.Add(row.InputConnector.NodeIOConnector, newNode);
            }

            if (row.OutputConnector is not null)
            {
                _connectorParents.Add(row.OutputConnector.NodeIOConnector, newNode);
            }
        }
    }
}
