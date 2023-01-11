using System;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

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
            context = context with { ExecutionFlags = context.ExecutionFlags & UIUpdateExecutionFlag.Value };
        }

        if (context.ExecutionSource is IIOConnector connectorSource)
        {
            ReadOnlySpan<IConditionalExecutionBranch> iter = new(_editableScript.NodeTree.GetExecutionBranches(connectorSource, context.ExecutionFlags));

            if (iter.Length == 1)
            {
                iter[0].Execute(context);
            }
            else
            {
                for (int i = 0; i < iter.Length; i++)
                {
                    iter[i].Execute(context);
                }
            }
        }
    }
}
