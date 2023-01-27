using System.Collections.Generic;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem.Contracts;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Implementation.Scripting.Execution;

internal class ExecutionOrderFinder
{
    List<IWrappedNode>? _currentBranchOrder;
    List<IOutputConnector>? _remainingBranchStarters;
    Dictionary<IOutputConnector, List<IWrappedNode>>? _connections;

    public ConditionalExecutionBranch[] FindExecutionPath(IOutputConnector firstConnector, ExecutionFlags flags, Dictionary<IOutputConnector, List<IWrappedNode>> connections)
    {
        List<ConditionalExecutionBranch> completedBranches = new();
        _connections = connections;
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

    public ConditionalExecutionBranch[] FindExecutionPath(IWrappedNode firstNode, ExecutionFlags flags, Dictionary<IOutputConnector, List<IWrappedNode>> connections)
    {
        _connections = connections;
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
        if (!_connections!.TryGetValue(currentBranchStarter, out List<IWrappedNode> currentNodeLevel))
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

    private List<IWrappedNode>? GetConnectionsIfBranchContinues(INodeRow row, ExecutionFlags flags)
    {
        if (row.OutputConnector is IOutputConnector outputConnector
            && _connections!.TryGetValue(outputConnector, out List<IWrappedNode> connectedNodes))
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
