using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Core.ScriptEditor;

internal class ScriptExecutionInstance : IScriptExecutionInstance
{
    private readonly IEditableScript _editableScript;

    public ScriptExecutionInstance(IEditableScript editableScript)
    {
        _editableScript = editableScript;
    }

    public ScriptState State { get; private set; } = ScriptState.Active;

    public bool IsShownInUI { get; set; } = true;

    public void TriggerNotification(LaminarExecutionContext context)
    {
        State = ScriptState.Running;

        if (IsShownInUI)
        {
            context = context with { ExecutionFlags = context.ExecutionFlags | ExecutionFlags.UpdateUI };
        }

        if (context.ExecutionSource is INodeWrapper nodeWrapper)
        {
            foreach (var node in _editableScript.NodeTree.GetExecutionOrder(nodeWrapper))
            {
                node.Update(context);
            }
        }
    }
}
