using System;
using System.Collections.Generic;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.Core.Extensions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Core.ScriptEditor;

public class NodeTree : INodeTree
{
    readonly ExecutionOrderFinder _executionOrderFinder = new();
    
    readonly Dictionary<INodeWrapper, List<INodeWrapper>> _dependentNodes = new();
    readonly Dictionary<INodeWrapper, INodeWrapper[]> _knownExecutionPaths = new();
    readonly Dictionary<IOutputConnector, List<INodeWrapper>> _connections = new();
    //readonly Dictionary<IIOConnector, Dictionary<int, ConditionalExecutionBranch[]>> _executionPaths = new();
    readonly Dictionary<(IIOConnector, int), ConditionalExecutionBranch[]> _executionPaths = new();
    readonly Dictionary<IIOConnector, INodeWrapper> _connectorParents = new();

    IReadOnlyList<INodeWrapper> _currentExecutionLevel;

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

        if (connector is IInputConnector)
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

        throw new ArgumentException($"Cannot start execution with object of type {connector.GetType()}", nameof(connector));
    }

    //private void AddExecutionPath(IIOConnector connector, int flags, ConditionalExecutionBranch[] result)
    //{
    //    if (_executionPaths.TryGetValue(connector, out Dictionary<int, ConditionalExecutionBranch[]> subDict))
    //    {
    //        subDict[flags] = result;
    //    }
    //    else
    //    {
    //        _executionPaths.Add(connector, new Dictionary<int, ConditionalExecutionBranch[]>() { { flags, result } });
    //    }
    //}

    public INodeWrapper[] GetExecutionOrder(INodeWrapper node)
    {
        if (_knownExecutionPaths.TryGetValue(node, out INodeWrapper[] executionOrder))
        {
            return executionOrder;
        }

        List<INodeWrapper> newExecutionOrder = new() { node };
        _currentExecutionLevel = GetDirectDependents(node);
        List<INodeWrapper> nextExecutionLevel = new();

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

        INodeWrapper[] newOrderArray = newExecutionOrder.ToArray();
        _knownExecutionPaths.Add(node, newOrderArray);
        return newOrderArray;
    }

    private IReadOnlyList<INodeWrapper> GetDirectDependents(INodeWrapper node) => _dependentNodes.TryGetValue(node, out List<INodeWrapper> dependentNodes) ? dependentNodes : Array.Empty<INodeWrapper>();

    private void ConnectionRemoved(object sender, IConnection removedConnection)
    {
        INodeWrapper outputNode = _connectorParents[removedConnection.OutputConnector];
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
        INodeWrapper outputNode = _connectorParents[newConnection.OutputConnector];
        if (!_dependentNodes.ContainsKey(outputNode))
        {
            _dependentNodes.Add(outputNode, new List<INodeWrapper>());
        }

        _dependentNodes[outputNode].Add(_connectorParents[newConnection.InputConnector]);

        if (!_connections.ContainsKey(newConnection.OutputConnector))
        {
            _connections.Add(newConnection.OutputConnector, new List<INodeWrapper>());
        }

        _connections[newConnection.OutputConnector].Add(_connectorParents[newConnection.InputConnector]);

        _knownExecutionPaths.Clear();
    }

    private void NodeRemoved(object sender, INodeWrapper removedNode)
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

    private void NodeAdded(object sender, INodeWrapper newNode)
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
