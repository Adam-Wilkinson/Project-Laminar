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

        if (_calculatedBranches.TryGetValue((context.ExecutionSource, context.ExecutionFlags.AsNumber), out OrderFinderInstance instance))
        {
            return instance.Branches();
        }

        OrderFinderInstance newFinder = new(context, tree);
        _calculatedBranches.Add((context.ExecutionSource, context.ExecutionFlags.AsNumber), newFinder);
        return newFinder.Branches();
    }

    private class OrderFinderInstance
    {
        readonly INodeTree _tree;
        readonly object _source;
        readonly ExecutionFlags _flags;
        readonly object _lockObject = new();

        IConditionalExecutionBranch[]? _lastCalculation;
        List<IOutputConnector>? _remainingBranchStarters;
        List<IWrappedNode>? _currentBranchOrder;

        public OrderFinderInstance(LaminarExecutionContext context, INodeTree tree)
        {
            _tree = tree;
            _flags = context.ExecutionFlags;
            _source = context.ExecutionSource!;

            _tree.Changed += (o, e) => _lastCalculation = null;
        }

        public IConditionalExecutionBranch[] Branches()
        {
            return _lastCalculation ??= FindExecutionPath(_source, _flags);
        }

        public IConditionalExecutionBranch[] FindExecutionPath(object source, ExecutionFlags flags)
        {
            lock (_lockObject)
            {
                if (source is IWrappedNode nodeSource)
                {
                    return FindExecutionPathFromNode(nodeSource, flags);
                }

                if (source is IOutputConnector outputConnector)
                {
                    return FindExecutionPathFromOutput(outputConnector, flags);
                }

                throw new Exception($"Could not make execution path from source {source}");
            }
        }

        public ConditionalExecutionBranch[] FindExecutionPathFromOutput(IOutputConnector firstConnector, ExecutionFlags flags)
        {
            List<ConditionalExecutionBranch> completedBranches = new();
            _remainingBranchStarters = new() { firstConnector };
            while (_remainingBranchStarters.Count > 0)
            {
                IOutputConnector currentBranchStarter = _remainingBranchStarters[0];
                _currentBranchOrder = new();
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new(_currentBranchOrder.ToArray(), currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                _remainingBranchStarters.RemoveAt(0);
            }

            return completedBranches.ToArray();
        }

        public ConditionalExecutionBranch[] FindExecutionPathFromNode(IWrappedNode firstNode, ExecutionFlags flags)
        {
            _remainingBranchStarters = new();

            _currentBranchOrder = new() { firstNode };
            foreach (INodeRow row in firstNode.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, flags) is not null)
                {
                    FindPathFromOutputConnector(row.OutputConnector!, flags);
                }
            }

            List<ConditionalExecutionBranch> completedBranches = new() { new ConditionalExecutionBranch(_currentBranchOrder.ToArray()) };

            while (_remainingBranchStarters.Count > 0)
            {
                IOutputConnector currentBranchStarter = _remainingBranchStarters[0];
                _currentBranchOrder = new();
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new(_currentBranchOrder.ToArray(), currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                _remainingBranchStarters.RemoveAt(0);
            }
            return completedBranches.ToArray();
        }

        private void FindPathFromOutputConnector(IOutputConnector currentBranchStarter, ExecutionFlags executionFlags)
        {
            if (_tree.GetConnections(currentBranchStarter) is not IReadOnlyList<IWrappedNode> currentNodeLevel)
            {
                return;
            }

            List<IWrappedNode> nextNodeLevel = new();

            while (currentNodeLevel.Count > 0)
            {
                foreach (IWrappedNode currentNode in currentNodeLevel)
                {
                    _currentBranchOrder!.Remove(currentNode);
                    _currentBranchOrder.Add(currentNode);
                    nextNodeLevel.AddRange(GetDependentNodes(currentNode, executionFlags));
                }
                currentNodeLevel = nextNodeLevel;
                nextNodeLevel = new();
            }
        }

        private IEnumerable<IWrappedNode> GetDependentNodes(IWrappedNode node, ExecutionFlags executionFlags)
        {
            foreach (INodeRow row in node.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, executionFlags) is List<IWrappedNode> connectedNodes)
                {
                    foreach (IWrappedNode connectedNode in connectedNodes)
                    {
                        yield return connectedNode;
                    }
                }
            }
        }

        private IReadOnlyList<IWrappedNode>? GetConnectionsIfBranchContinues(INodeRow row, ExecutionFlags flags)
        {
            if (row.OutputConnector is IOutputConnector outputConnector
                && _tree.GetConnections(outputConnector) is IReadOnlyList<IWrappedNode> connectedNodes)
            {
                switch (outputConnector.PassUpdate(flags))
                {
                    case PassUpdateOption.AlwaysPasses:
                        return connectedNodes;
                    case PassUpdateOption.CurrentlyPasses:
                    case PassUpdateOption.CurrentlyDoesNotPass:
                        _remainingBranchStarters!.Add(outputConnector);
                        break;

                }
            }

            return null;
        }
    }
}
