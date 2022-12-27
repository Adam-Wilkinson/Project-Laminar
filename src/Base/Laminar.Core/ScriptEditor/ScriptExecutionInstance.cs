using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            context.ExecutionFlags.AddFlag(UIUpdateExecutionFlag.Value);
        }

        if (context.ExecutionSource is INodeWrapper nodeWrapper)
        {
            Span<INodeWrapper> iter = _editableScript.NodeTree.GetExecutionOrder(nodeWrapper);
            for (int i = 0; i < iter.Length; i++)
            {
                iter[i].Update(context);
            }
        }
    }
}
