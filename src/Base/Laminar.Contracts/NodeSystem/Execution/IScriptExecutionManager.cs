namespace Laminar.Contracts.NodeSystem.Execution;

public interface IScriptExecutionManager
{
    public IEnumerable<IScriptExecutionInstance> AllInstances { get; }

    public IScriptExecutionInstance CreateExecutionInstance(IEditableScript editableScript);

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance);
}
