namespace Laminar.Contracts.Scripting.Execution;

public interface IScriptExecutionManager
{
    public IEnumerable<IScriptExecutionInstance> AllInstances { get; }

    public IScriptExecutionInstance CreateExecutionInstance(INodeTree nodeTree);

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance);
}
