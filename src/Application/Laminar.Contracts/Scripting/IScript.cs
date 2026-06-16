using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Contracts.Scripting;

public interface IScript
{
    public string Name { get; set; }

    public INodeTree WritableNodeTree { get; }

    public IScriptExecutionInstance ExecutionInstance { get; }

    public ScriptState State { get; }
}
