namespace Laminar.Contracts.Scripting.Execution;

public interface IExecutionManager
{
    public IEnumerable<IScriptExecutionInstance> AllInstances { get; }

    public IScriptExecutionInstance CreateExecutionInstance(INodeGraph nodeGraph);

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance);
}
