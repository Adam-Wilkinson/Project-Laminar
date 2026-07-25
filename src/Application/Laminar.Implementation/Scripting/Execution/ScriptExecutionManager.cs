using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionManager(IExecutionOrderFinder executionOrderFinder) : IScriptExecutionManager
{
    private readonly List<IScriptExecutionInstance> _instances = [];

    public IEnumerable<IScriptExecutionInstance> AllInstances => _instances;

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance) => _instances.Remove(executionInstance);

    public IScriptExecutionInstance CreateExecutionInstance(INodeTree nodeTree)
    {
        IScriptExecutionInstance newInstance = new ScriptExecutionInstance(nodeTree, executionOrderFinder);
        _instances.Add(newInstance);
        return newInstance;
    }
}
