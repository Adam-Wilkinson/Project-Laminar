using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionManager(IExecutionOrderFinder executionOrderFinder) : IScriptExecutionManager
{
    private readonly List<IScriptExecutionInstance> _instances = [];

    public IEnumerable<IScriptExecutionInstance> AllInstances => _instances;

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance) => _instances.Remove(executionInstance);

    public IScriptExecutionInstance CreateExecutionInstance(INodeTreeView nodeTreeView)
    {
        IScriptExecutionInstance newInstance = new ScriptExecutionInstance(nodeTreeView, executionOrderFinder);
        _instances.Add(newInstance);
        return newInstance;
    }
}
