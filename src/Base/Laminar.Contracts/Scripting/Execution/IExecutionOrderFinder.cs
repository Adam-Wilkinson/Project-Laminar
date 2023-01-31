using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

public interface IExecutionOrderFinder
{
    public IConditionalExecutionBranch[] GetExecutionBranchesFrom(LaminarExecutionContext context, INodeTree tree);
}
