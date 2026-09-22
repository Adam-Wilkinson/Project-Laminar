using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Components;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

internal sealed class ExecutionOrderFinder : IExecutionOrderFinder
{
    public IConditionalExecutionBranch[] ComputeExecutionBranchesFrom(LaminarExecutionContext context, INodeGraph graph)
    {
        if (context.ExecutionSource is null)
            throw new InvalidOperationException("Source cannot be null");
        
        List<IOutputConnector> remainingBranchStarters;
        List<INodeContainer> currentBranchOrder;
        
        return FindExecutionPath(context.ExecutionSource, context.ExecutionFlags);

        IConditionalExecutionBranch[] FindExecutionPath(object source, ExecutionFlags flags) => source switch
        {
            INodeContainer nodeSource => FindExecutionPathFromNode(nodeSource, flags),
            IOutputConnector outputConnector => FindExecutionPathFromOutput(outputConnector, flags),
            _ => throw new Exception($"Could not make execution path from source {source}")
        };
        
        IConditionalExecutionBranch[] FindExecutionPathFromOutput(IOutputConnector firstConnector, ExecutionFlags flags)
        {
            List<IConditionalExecutionBranch> completedBranches = [];
            remainingBranchStarters = [firstConnector];
            while (remainingBranchStarters.Count > 0)
            {
                var currentBranchStarter = remainingBranchStarters[0];
                currentBranchOrder = [];
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new([.. currentBranchOrder], currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                remainingBranchStarters.RemoveAt(0);
            }

            return [.. completedBranches];
        }

        IConditionalExecutionBranch[] FindExecutionPathFromNode(INodeContainer firstNodeContainer, ExecutionFlags flags)
        {
            remainingBranchStarters = [];
            currentBranchOrder = [firstNodeContainer];
            foreach (var row in firstNodeContainer.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, flags) is not null)
                {
                    FindPathFromOutputConnector(row.OutputConnector!, flags);
                }
            }

            List<IConditionalExecutionBranch> completedBranches = 
                [new ConditionalExecutionBranch([.. currentBranchOrder])];

            while (remainingBranchStarters.Count > 0)
            {
                var currentBranchStarter = remainingBranchStarters[0];
                currentBranchOrder = [];
                FindPathFromOutputConnector(currentBranchStarter, flags);
                ConditionalExecutionBranch recentlyFoundBranch = new([.. currentBranchOrder], currentBranchStarter);
                completedBranches.Add(recentlyFoundBranch);
                remainingBranchStarters.RemoveAt(0);
            }
            return [.. completedBranches];
        }

        void FindPathFromOutputConnector(IOutputConnector currentBranchStarter, ExecutionFlags executionFlags)
        {
            var currentConnectionsLevel = graph.GetConnectionsTo(currentBranchStarter);
            List<ConnectorConnectionInfo> nextConnectionsLevel = [];

            while (currentConnectionsLevel.Count > 0)
            {
                foreach (var currentConnections in currentConnectionsLevel)
                {
                    var currentNode = currentConnections.ConnectedNodeContainer;
                    currentBranchOrder.Remove(currentNode);
                    currentBranchOrder.Add(currentNode);
                    nextConnectionsLevel.AddRange(GetDependentNodes(currentNode, executionFlags));
                }
                
                currentConnectionsLevel = nextConnectionsLevel;
                nextConnectionsLevel = [];
            }
        }

        IEnumerable<ConnectorConnectionInfo> GetDependentNodes(INodeContainer nodeContainer, ExecutionFlags executionFlags)
        {
            foreach (var row in nodeContainer.Rows)
            {
                if (GetConnectionsIfBranchContinues(row, executionFlags) is not { } connectedNodes) continue;
                
                foreach (var connectedNode in connectedNodes)
                {
                    yield return connectedNode;
                }
            }
        }

        IReadOnlyCollection<ConnectorConnectionInfo>? GetConnectionsIfBranchContinues(INodeRow row, ExecutionFlags flags)
        {
            if (row.OutputConnector is not { } outputConnector
                || graph.GetConnectionsTo(outputConnector) is not { } connections)
                return null;
            
            switch (outputConnector.PassUpdate(flags))
            {
                case PassUpdateOption.AlwaysPasses:
                    return connections;
                case PassUpdateOption.CurrentlyPasses:
                case PassUpdateOption.CurrentlyDoesNotPass:
                    remainingBranchStarters!.Add(outputConnector);
                    return null;
                case PassUpdateOption.NeverPasses:
                default:
                    return null;
            }
        }
    }
}
