using System;
using System.Collections.Generic;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Connection;
using Laminar.Core.Extensions;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Core.ScriptEditor;

public class NodeTree : INodeTree
{
    readonly Dictionary<INodeWrapper, List<INodeWrapper>> _dependentNodes = new();
    readonly Dictionary<INodeWrapper, INodeWrapper[]> _knownExecutionPaths = new();
    readonly Dictionary<IOutputConnector, List<INodeWrapper>> _connections = new();
    readonly Dictionary<(IIOConnector, ExecutionFlags), ConditionalExecutionBranch[]> _executionPaths = new();
    readonly Dictionary<IIOConnector, INodeWrapper> _connectorParents = new();

    List<INodeWrapper>? _currentExecutionTree;
    IReadOnlyList<INodeWrapper> _currentExecutionLevel;

    public NodeTree(IScript script)
    {
        script.Nodes.ItemAdded += NodeAdded;
        script.Nodes.ItemRemoved += NodeRemoved;

        script.Connections.ItemAdded += ConnectionAdded;
        script.Connections.ItemRemoved += ConnectionRemoved;
    }

    public IReadOnlyList<INodeWrapper> GetDirectDependents(INodeWrapper node) => _dependentNodes.TryGetValue(node, out List<INodeWrapper> dependentNodes) ? dependentNodes : Array.Empty<INodeWrapper>();

    public ConditionalExecutionBranch[] GetExecutionBranches(IIOConnector connector, ExecutionFlags flags)
    {
        if (_executionPaths.TryGetValue((connector, flags), out ConditionalExecutionBranch[] branches))
        {
            return branches;
        }

        if (connector is IInputConnector)
        {
            ConditionalExecutionBranch[] result = GetBranchesFromNodes(_connectorParents[connector].Yield(), flags);
            _executionPaths.Add((connector, flags), result);
            return result;
        }

        if (connector is IOutputConnector outputConnector)
        {
            ConditionalExecutionBranch[] result = GetBranchesFromNodes(_connections[outputConnector], flags);
            _executionPaths.Add((connector, flags), result);
            return result;
        }

        throw new ArgumentException($"Cannot start execution with object of type {connector.GetType()}", nameof(connector));
    }

    private ConditionalExecutionBranch[] GetBranchesFromNodes(IEnumerable<INodeWrapper> firstExecutionLevel, ExecutionFlags flags)
    {
        List<ConditionalExecutionBranch> result = new();
        List<IOutputConnector> remainingBranchStarters = new();
        //List<INodeWrapper>currentBranchOrder = new();
        List<INodeWrapper> nextExecutionLevel = new();

        foreach (INodeWrapper node in firstExecutionLevel)
        {

        }


        while (remainingBranchStarters.Count > 0)
        {
            IOutputConnector currentBranchStarter = remainingBranchStarters[0];
            List<INodeWrapper> currentBranchOrder = new();
            if (_connections.TryGetValue(currentBranchStarter, out List<INodeWrapper> connectedNodes))
            {
                foreach (INodeWrapper node in connectedNodes)
                {
                    currentBranchOrder.Add(node);
                    foreach (INodeRowWrapper row in node.Fields)
                    {
                        if (row.OutputConnector is IOutputConnector currentOutputConnector
                            && _connections.TryGetValue(currentOutputConnector, out List<INodeWrapper> nextNodes))
                        {
                            if (currentOutputConnector.ActivitySetting is ActivitySetting.AlwaysActive)
                            {
                                currentBranchOrder.AddRange(nextNodes);
                            }
                            else if (currentOutputConnector.ActivitySetting is ActivitySetting.CurrentlyActive or ActivitySetting.Inactive)
                            {
                                remainingBranchStarters.Add(currentOutputConnector);
                            }
                        }
                    }
                }
            }
        }
        while (currentBranchOrder.Count > 0)
        {
            foreach (INodeWrapper node in currentBranchOrder)
            {
                currentBranchOrder.Add(node);
                foreach (INodeRowWrapper row in node.Fields)
                {
                    if (row.OutputConnector is IOutputConnector outputConnector 
                        && outputConnector.ActivitySetting == ActivitySetting.AlwaysActive 
                        && _connections.TryGetValue(outputConnector, out List<INodeWrapper> nextNodes))
                    {
                        nextExecutionLevel.AddRange(nextNodes);
                    }
                }
            }
        }

        return result.ToArray();
    }

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
