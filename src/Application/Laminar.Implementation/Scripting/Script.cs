using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript
{
    private readonly IWritableNodeTree _writableNodeTree;
    
    public Script(IScriptExecutionManager executionManager, IWritableNodeTree writableNodeTree)
    {
        _writableNodeTree = writableNodeTree;
        ExecutionInstance = executionManager.CreateExecutionInstance(WritableNodeTree);
    }

    public string Name { get; set; } = "Unnamed Script";

    public Contracts.Scripting.INodeTree WritableNodeTree => _writableNodeTree;

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }
}
