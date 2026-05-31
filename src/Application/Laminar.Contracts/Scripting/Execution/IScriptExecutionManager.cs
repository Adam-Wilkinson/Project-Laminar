namespace Laminar.Contracts.Scripting.Execution;

public interface IScriptExecutionManager
{
    public IEnumerable<IScriptExecutionInstance> AllInstances { get; }

    public IScriptExecutionInstance CreateExecutionInstance(INodeTreeView nodeTreeView);

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance);
}
