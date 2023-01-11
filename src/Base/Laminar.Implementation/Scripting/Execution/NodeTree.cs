using System;
using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Connection;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Implementation.Extensions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Execution;

public class NodeTree : INodeTree
{
    readonly ExecutionOrderFinder _executionOrderFinder = new();

    readonly Dictionary<IWrappedNode, List<IWrappedNode>> _dependentNodes = new();
    readonly Dictionary<IWrappedNode, IWrappedNode[]> _knownExecutionPaths = new();
    readonly Dictionary<IOutputConnector, List<IWrappedNode>> _connections = new();
    //readonly Dictionary<IIOConnector, Dictionary<int, ConditionalExecutionBranch[]>> _executionPaths = new();
    readonly Dictionary<(IIOConnector, int), ConditionalExecutionBranch[]> _executionPaths = new();
    readonly Dictionary<IIOConnector, IWrappedNode> _connectorParents = new();

    IReadOnlyList<IWrappedNode> _currentExecutionLevel;

    public NodeTree(IScript script)
    {
        script.Nodes.ItemAdded += NodeAdded;
        script.Nodes.ItemRemoved += NodeRemoved;

        script.Connections.ItemAdded += ConnectionAdded;
        script.Connections.ItemRemoved += ConnectionRemoved;
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

    public IWrappedNode[] GetExecutionOrder(IWrappedNode node)
    {
        if (_knownExecutionPaths.TryGetValue(node, out IWrappedNode[] executionOrder))
        {
            return executionOrder;
        }

        List<IWrappedNode> newExecutionOrder = new() { node };
        _currentExecutionLevel = GetDirectDependents(node);
        List<IWrappedNode> nextExecutionLevel = new();

        while (_currentExecutionLevel.Count > 0)
        {
            foreach (var currentNode in _currentExecutionLevel)
            {
                newExecutionOrder.Add(currentNode);
                nextExecutionLevel.AddRange(GetDirectDependents(currentNode));
            }
            _currentExecutionLevel = nextExecutionLevel;
            nextExecutionLevel = new();
        }

        IWrappedNode[] newOrderArray = newExecutionOrder.ToArray();
        _knownExecutionPaths.Add(node, newOrderArray);
        return newOrderArray;
    }

    private IReadOnlyList<IWrappedNode> GetDirectDependents(IWrappedNode node) => _dependentNodes.TryGetValue(node, out List<IWrappedNode> dependentNodes) ? dependentNodes : Array.Empty<IWrappedNode>();

    private void ConnectionRemoved(object sender, IConnection removedConnection)
    {
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

    private void ConnectionAdded(object sender, IConnection newConnection)
    {
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

    private void NodeRemoved(object sender, IWrappedNode removedNode)
    {
        foreach (var field in removedNode.Fields)
        {
            if (field.InputConnector is not null)
            {
                _connectorParents.Remove(field.InputConnector.NodeIOConnector);
            }

            if (field.OutputConnector is not null)
            {
                _connectorParents.Remove(field.OutputConnector.NodeIOConnector);
            }
        }
    }

    private void NodeAdded(object sender, IWrappedNode newNode)
    {
        foreach (var field in newNode.Fields)
        {
            if (field.InputConnector is not null)
            {
                _connectorParents.Add(field.InputConnector.NodeIOConnector, newNode);
            }

            if (field.OutputConnector is not null)
            {
                _connectorParents.Add(field.OutputConnector.NodeIOConnector, newNode);
            }
        }
    }
}
