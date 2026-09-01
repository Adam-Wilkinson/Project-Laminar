using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Contracts.Scripting.Execution;

public interface IConditionalExecutionBranch
{
    public ExecutionResult Execute(LaminarExecutionContext context);
}

public struct ExecutionResult
{
    public static ExecutionResult Success { get; } = new() { Status = ExecutionResultStatus.Success };
    
    public static ExecutionResult DidNotExecute { get; } = new() { Status = ExecutionResultStatus.DidNotExecute };

    public static ExecutionResult Error(Exception exception) =>
        new() { Status = ExecutionResultStatus.Error, Exception = exception };
    
    public required ExecutionResultStatus Status { get; init; }
    
    public Exception? Exception { get; private init; }
}

public enum ExecutionResultStatus
{
    Success,
    DidNotExecute,
    Error
}