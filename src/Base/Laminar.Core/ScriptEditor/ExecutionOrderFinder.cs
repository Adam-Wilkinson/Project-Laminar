using System.Collections.Generic;
using Laminar.Contracts.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;
using Laminar.PluginFramework.NodeSystem.ExecutionFlags;

namespace Laminar.Core.ScriptEditor;

internal class ExecutionOrderFinder
{
    List<INodeWrapper> _currentBranchOrder;
    List<IOutputConnector> _remainingBranchStarters;
    Dictionary<IOutputConnector, List<INodeWrapper>> _connections;

    public ConditionalExecutionBranch[] FindExecutionPath(IOutputConnector firstConnector, ExecutionFlags flags, Dictionary<IOutputConnector, List<INodeWrapper>> connections)
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

    public ConditionalExecutionBranch[] FindExecutionPath(INodeWrapper firstNode, ExecutionFlags flags, Dictionary<IOutputConnector, List<INodeWrapper>> connections)
    {
        _connections = connections;
        _remainingBranchStarters = new();

        _currentBranchOrder = new() { firstNode };
        foreach (INodeRowWrapper row in firstNode.Fields)
        {
            if (GetConnectionsIfBranchContinues(row, flags) is not null)
            {
                FindPathFromOutputConnector((IOutputConnector)(row.OutputConnector.NodeIOConnector), flags);
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
        if (!_connections.TryGetValue(currentBranchStarter, out List<INodeWrapper> currentNodeLevel))
        {
            return;
        }

        List<INodeWrapper> nextNodeLevel = new();

        while (currentNodeLevel.Count > 0)
        {
            foreach (INodeWrapper currentNode in currentNodeLevel)
            {
                _currentBranchOrder.Remove(currentNode);
                _currentBranchOrder.Add(currentNode);
                nextNodeLevel.AddRange(GetDependentNodes(currentNode, executionFlags));
            }
            currentNodeLevel = nextNodeLevel;
            nextNodeLevel = new();
        }
    }

    private IEnumerable<INodeWrapper> GetDependentNodes(INodeWrapper node, ExecutionFlags executionFlags)
    {
        foreach (INodeRowWrapper row in node.Fields)
        {
            if (GetConnectionsIfBranchContinues(row, executionFlags) is List<INodeWrapper> connectedNodes)
            {
                foreach (INodeWrapper connectedNode in connectedNodes)
                {
                    yield return connectedNode;
                }
            }
        }
    }

    private List<INodeWrapper>? GetConnectionsIfBranchContinues(INodeRowWrapper row, ExecutionFlags flags)
    {
        if (row.OutputConnector?.NodeIOConnector is IOutputConnector outputConnector
            && _connections.TryGetValue(outputConnector, out List<INodeWrapper> connectedNodes))
        {
            switch (outputConnector.PassUpdate(flags))
            {
                case PassUpdateOption.AlwaysPasses:
                    return connectedNodes;
                case PassUpdateOption.CurrentlyPasses:
                case PassUpdateOption.CurrentlyDoesNotPass:
                    _remainingBranchStarters.Add(outputConnector);
                    break;

            }
        }

        return null;
    }
}
