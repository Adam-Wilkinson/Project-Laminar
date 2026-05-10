using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

public interface IStartExecutionTarget
{
    public void StartExecution(object? source, ExecutionFlags flags, uint timestamp);
}