using System;
using System.Collections.Generic;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

internal class ExecutionOrderFinder : IExecutionOrderFinder
{
    private readonly Dictionary<(object, int), OrderFinderInstance> _calculatedBranches = new();

    public IConditionalExecutionBranch[] GetExecutionBranchesFrom(LaminarExecutionContext context, INodeTree tree)
    {
        ArgumentNullException.ThrowIfNull(context.ExecutionSource);

        if (_calculatedBranches.TryGetValue((context.ExecutionSource, context.ExecutionFlags.AsNumber), out var instance))
        {
            return instance.Branches();
        }

        OrderFinderInstance newFinder = new(context, tree);
        _calculatedBranches.Add((context.ExecutionSource, context.ExecutionFlags.AsNumber), newFinder);
        return newFinder.Branches();
    }

    private class OrderFinderInstance
    {
        private readonly INodeTree _tree;
        private readonly object _source;
        private readonly ExecutionFlags _flags;
        private readonly Lock _lockObject = new();

        private IConditionalExecutionBranch[]? _lastCalculation;
        private List<IOutputConnector>? _remainingBranchStarters;
        private List<IWrappedNode>? _currentBranchOrder;

        public OrderFinderInstance(LaminarExecutionContext context, INodeTree tree)
        {
            _tree = tree;
            _flags = context.ExecutionFlags;
            _source = context.ExecutionSource!;

            _tree.Changed += (_, _) => _lastCalculation = null;
        }

        public IConditionalExecutionBranch[] Branches()
        {
            return _lastCalculation ??= FindExecutionPath(_source, _flags);
        }

        private IConditionalExecutionBranch[] FindExecutionPath(object source, ExecutionFlags flags)
        {
            lock (_lockObject)
            {
                return source switch
                {
                    IWrappedNode nodeSource => FindExecutionPathFromNode(nodeSource, flags),
                    IOutputConnector outputConnector => FindExecutionPathFromOutput(outputConnector, flags),
                    _ => throw new Exception($"Could not make execution path from source {source}")
                };
            }
        }

        private IConditionalExecutionBranch[] FindExecutionPathFromOutput(IOutputConnector firstConnector, ExecutionFlags flags)
        {
            List<IConditionalExecutionBranch> completedBranches = [];
            _remainingBranchStarters = [firstConnector];
            while (_remainingBranchStarters.Count > 0)
            {
                IOutputConnector currentBranchStarter = _remainingBranchStarters[0];
                _currentBranchOrder = [];
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new(_currentBranchOrder.ToArray(), currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                _remainingBranchStarters.RemoveAt(0);
            }

            return completedBranches.ToArray();
        }

        private IConditionalExecutionBranch[] FindExecutionPathFromNode(IWrappedNode firstNode, ExecutionFlags flags)
        {
            _remainingBranchStarters = [];
            _currentBranchOrder = [firstNode];
            foreach (INodeRow row in firstNode.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, flags) is not null)
                {
                    FindPathFromOutputConnector(row.OutputConnector!, flags);
                }
            }

            List<IConditionalExecutionBranch> completedBranches = 
                [new ConditionalExecutionBranch(_currentBranchOrder.ToArray())];

            while (_remainingBranchStarters.Count > 0)
            {
                IOutputConnector currentBranchStarter = _remainingBranchStarters[0];
                _currentBranchOrder = [];
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new(_currentBranchOrder.ToArray(), currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                _remainingBranchStarters.RemoveAt(0);
            }
            return completedBranches.ToArray();
        }

        private void FindPathFromOutputConnector(IOutputConnector currentBranchStarter, ExecutionFlags executionFlags)
        {
            var currentConnectionsLevel = _tree.GetConnections(currentBranchStarter);
            List<ConnectorConnectionInfo> nextConnectionsLevel = [];

            while (currentConnectionsLevel.Count > 0)
            {
                foreach (var currentConnections in currentConnectionsLevel)
                {
                    var currentNode = currentConnections.ConnectedNode;
                    _currentBranchOrder!.Remove(currentNode);
                    _currentBranchOrder.Add(currentNode);
                    nextConnectionsLevel.AddRange(GetDependentNodes(currentNode, executionFlags));
                }
                
                currentConnectionsLevel = nextConnectionsLevel;
                nextConnectionsLevel = [];
            }
        }

        private IEnumerable<ConnectorConnectionInfo> GetDependentNodes(IWrappedNode node, ExecutionFlags executionFlags)
        {
            foreach (INodeRow row in node.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, executionFlags) is not { } connectedNodes) continue;
                
                foreach (var connectedNode in connectedNodes)
                {
                    yield return connectedNode;
                }
            }
        }

        private IReadOnlyCollection<ConnectorConnectionInfo>? GetConnectionsIfBranchContinues(INodeRow row, ExecutionFlags flags)
        {
            if (row.OutputConnector is not { } outputConnector
                || _tree.GetConnections(outputConnector) is not { } connections)
                return null;
            
            switch (outputConnector.PassUpdate(flags))
            {
                case PassUpdateOption.AlwaysPasses:
                    return connections;
                case PassUpdateOption.CurrentlyPasses:
                case PassUpdateOption.CurrentlyDoesNotPass:
                    _remainingBranchStarters!.Add(outputConnector);
                    return null;
                case PassUpdateOption.NeverPasses:
                default:
                    return null;
            }
        }
    }
}
