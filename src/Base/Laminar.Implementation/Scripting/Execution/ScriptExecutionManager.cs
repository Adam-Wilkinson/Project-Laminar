using System.Collections.Generic;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionManager : IScriptExecutionManager
{
    private readonly List<IScriptExecutionInstance> _instances = new();
    private readonly IExecutionOrderFinder _executionOrderFinder;

    public ScriptExecutionManager(IExecutionOrderFinder executionOrderFinder)
    {
        _executionOrderFinder = executionOrderFinder;
    }

    public IEnumerable<IScriptExecutionInstance> AllInstances => _instances;

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance) => _instances.Remove(executionInstance);

    public IScriptExecutionInstance CreateExecutionInstance(IEditableScript editableScript)
    {
        IScriptExecutionInstance newInstance = new ScriptExecutionInstance(editableScript, _executionOrderFinder);
        _instances.Add(newInstance);
        return newInstance;
    }
}
