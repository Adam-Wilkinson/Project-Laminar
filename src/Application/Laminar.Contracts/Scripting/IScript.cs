using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Contracts.Scripting;

public interface IScript
{
    public INodeTree WritableNodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }
}
