using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting.Execution;

internal class ExecutionManager(IExecutionOrderFinder executionOrderFinder, IExceptionHandler exceptionHandler) 
    : IExecutionManager
{
    private readonly List<IScriptExecutionInstance> _instances = [];

    public IEnumerable<IScriptExecutionInstance> AllInstances => _instances;

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance) => _instances.Remove(executionInstance);

    public IScriptExecutionInstance CreateExecutionInstance(INodeGraph nodeGraph)
    {
        var newInstance = new ScriptExecutionInstance(nodeGraph, executionOrderFinder, exceptionHandler);
        _instances.Add(newInstance);
        return newInstance;
    }
}
