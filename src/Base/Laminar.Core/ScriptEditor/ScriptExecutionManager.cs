using System.Collections.Generic;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Core.ScriptEditor;

internal class ScriptExecutionManager : IScriptExecutionManager
{
    private readonly List<IScriptExecutionInstance> _instances = new();

    public IEnumerable<IScriptExecutionInstance> AllInstances => _instances;

    public bool DestroyExecutionInstance(IScriptExecutionInstance executionInstance) => _instances.Remove(executionInstance);

    public IScriptExecutionInstance CreateExecutionInstance(IEditableScript editableScript)
    {
        IScriptExecutionInstance newInstance = new ScriptExecutionInstance(editableScript);
        _instances.Add(newInstance);
        return newInstance;
    }
}
