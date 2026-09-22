using Laminar.Contracts.Base;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.Execution;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Implementation.Scripting.Execution;

internal sealed class ScriptExecutionInstance : IScriptExecutionInstance
{
    private readonly Dictionary<ExecutionContextIdentity, IConditionalExecutionBranch[]> _calculatedBranches = [];
    private readonly INodeGraph _nodeGraph;
    private readonly IExecutionOrderFinder _orderFinder;
    private readonly IExceptionHandler _exceptionHandler;

    public ScriptExecutionInstance(INodeGraph nodeGraph, 
        IExecutionOrderFinder orderFinder, 
        IExceptionHandler exceptionHandler)
    {
        _nodeGraph = nodeGraph;
        _orderFinder = orderFinder;
        _exceptionHandler = exceptionHandler;
        _nodeGraph.Changed += NodeGraphOnChanged;
    }

    public ScriptState State { get; private set; } = ScriptState.Active;

    public bool IsShownInUI { get; set; } = true;

    public void TriggerNotification(LaminarExecutionContext context)
    {        
        State = ScriptState.Running;

        if (IsShownInUI)
        {
            context = context with { ExecutionFlags = context.ExecutionFlags | UiUpdateExecutionFlag.Value };
        }

        if (context.ExecutionSource is null)
        {
            throw new InvalidOperationException("Cannot execute from unknown source");
        }
        
        var contextIdentity = new ExecutionContextIdentity(context.ExecutionSource, context.ExecutionFlags);
        if (!_calculatedBranches.TryGetValue(contextIdentity, out var branches))
        {
            branches = _orderFinder.ComputeExecutionBranchesFrom(context, _nodeGraph);
            _calculatedBranches[contextIdentity] = branches;
        }
        
        ReadOnlySpan<IConditionalExecutionBranch> iter = new(branches);

        if (iter.Length == 1)
        {
            if (iter[0].Execute(context).Exception is not { } exception) return;
            _exceptionHandler.OnException(exception);
            return;
        }
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < iter.Length; i++)
        {
            if (iter[i].Execute(context).Exception is not { } exception) continue;
            _exceptionHandler.OnException(exception);
            break;
        }
    }
    
    public void Dispose()
    {
         _nodeGraph.Changed -= NodeGraphOnChanged;
         _calculatedBranches.Clear();
    }

    private void NodeGraphOnChanged(object? sender, EventArgs e)
    {
        _calculatedBranches.Clear();
    }
    
    private record struct ExecutionContextIdentity(object Source, ExecutionFlags Flags);
}
