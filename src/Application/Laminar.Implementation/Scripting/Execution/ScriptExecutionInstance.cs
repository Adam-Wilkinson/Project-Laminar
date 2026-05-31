using Laminar.Contracts.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionInstance(INodeTreeView nodeTreeView, IExecutionOrderFinder orderFinder)
    : IScriptExecutionInstance
{
    public ScriptState State { get; private set; } = ScriptState.Active;

    public bool IsShownInUI { get; set; } = true;

    public void TriggerNotification(LaminarExecutionContext context)
    {
        State = ScriptState.Running;

        if (IsShownInUI)
        {
            context = context with { ExecutionFlags = context.ExecutionFlags | UiUpdateExecutionFlag.Value };
        }

        ReadOnlySpan<IConditionalExecutionBranch> iter = new(orderFinder.GetExecutionBranchesFrom(context, nodeTreeView));

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
