using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Implementation.Scripting.Execution;

namespace Laminar.Implementation.Scripting;

internal class Script : IScript
{
    private readonly INodeTree _nodeTree;
    
    public Script(IScriptExecutionManager executionManager)
    {
        _nodeTree = new NodeTree();
        ExecutionInstance = executionManager.CreateExecutionInstance(NodeTreeView);
    }

    public string Name { get; set; } = "Unnamed Script";

    public INodeTreeView NodeTreeView => _nodeTree;

    public ScriptState State => ExecutionInstance.State;

    public IScriptExecutionInstance ExecutionInstance { get; }
}
