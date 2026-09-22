using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

internal class ScriptExecutionInstance(
    INodeGraph nodeGraph, 
    IExecutionOrderFinder orderFinder, 
    IExceptionHandler exceptionHandler) 
    : IScriptExecutionInstance
{
    private bool _isDisposed;

    public ScriptState State { get; private set; } = ScriptState.Active;

    public bool IsShownInUI { get; set; } = true;

    public void TriggerNotification(LaminarExecutionContext context)
    {
        if (_isDisposed) return;
        
        State = ScriptState.Running;

        if (IsShownInUI)
        {
            context = context with { ExecutionFlags = context.ExecutionFlags | UiUpdateExecutionFlag.Value };
        }

        ReadOnlySpan<IConditionalExecutionBranch> iter = new(orderFinder.GetExecutionBranchesFrom(context, nodeGraph));

        if (iter.Length == 1)
        {
            if (iter[0].Execute(context).Exception is not { } exception) return;
            exceptionHandler.OnException(exception);
            return;
        }
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < iter.Length; i++)
        {
            if (iter[i].Execute(context).Exception is not { } exception) continue;
            exceptionHandler.OnException(exception);
            break;
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
    }
}
