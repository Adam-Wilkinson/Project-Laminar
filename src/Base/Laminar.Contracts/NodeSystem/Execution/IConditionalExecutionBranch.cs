using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.NodeSystem.Execution;

public interface IConditionalExecutionBranch
{
    public void Execute(LaminarExecutionContext context);
}
