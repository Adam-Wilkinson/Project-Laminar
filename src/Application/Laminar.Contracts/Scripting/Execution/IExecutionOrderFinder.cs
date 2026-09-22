using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

public interface IExecutionOrderFinder
{
    public IConditionalExecutionBranch[] ComputeExecutionBranchesFrom(LaminarExecutionContext context, INodeGraph graph);
}
