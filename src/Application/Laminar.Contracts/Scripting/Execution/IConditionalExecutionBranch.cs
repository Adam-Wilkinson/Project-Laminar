using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

public interface IConditionalExecutionBranch
{
    public void Execute(LaminarExecutionContext context);
}
