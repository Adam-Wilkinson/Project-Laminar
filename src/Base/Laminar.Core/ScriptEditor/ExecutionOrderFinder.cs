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
            FindPathFromOutputConnector(currentBranchStarter);
            ConditionalExecutionBranch recentlyFoundBranch = new(_currentBranchOrder.ToArray(), () => currentBranchStarter.ActivitySetting is ActivitySetting.AlwaysActive or ActivitySetting.CurrentlyActive);
            completedBranches.Add(recentlyFoundBranch);
            _remainingBranchStarters.RemoveAt(0);
        }

        return completedBranches.ToArray();
    }

    private void FindPathFromOutputConnector(IOutputConnector currentBranchStarter)
    {
        if (_connections.TryGetValue(currentBranchStarter, out List<INodeWrapper> connectedNodes))
        {
            foreach (INodeWrapper node in connectedNodes)
            {
                FindExecutionFromNode(node);
            }
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
        }`
    }

    private void FindExecutionFromNode(INodeWrapper node)
    {
        _currentBranchOrder.Add(node);
        foreach (INodeRowWrapper row in node.Fields)
        {
            FindExecutionsFromRow(row);
        }
    }

    private void FindExecutionsFromRow(INodeRowWrapper row)
    {
        if (row.OutputConnector is IOutputConnector currentOutputConnector
            && _connections.TryGetValue(currentOutputConnector, out List<INodeWrapper> nextNodes))
        {
            if (currentOutputConnector.ActivitySetting is ActivitySetting.AlwaysActive)
            {
                _currentBranchOrder.AddRange(nextNodes);
            }
            else if (currentOutputConnector.ActivitySetting is ActivitySetting.CurrentlyActive or ActivitySetting.Inactive)
            {
                _remainingBranchStarters.Add(currentOutputConnector);
            }
        }
    }
}
