using System;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

public class ConditionalExecutionBranch : IConditionalExecutionBranch
{
    private readonly IWrappedNode[] _nodes;
    private readonly IOutputConnector? _startingConnector;

    public ConditionalExecutionBranch(IWrappedNode[] nodes)
    {
        _nodes = nodes;
    }

    public ConditionalExecutionBranch(IWrappedNode[] nodes, IOutputConnector startingConnector)
    {
        _startingConnector = startingConnector;
        _nodes = nodes;
    }

    public void Execute(LaminarExecutionContext context)
    {
        if (_startingConnector is not null &&
            _startingConnector.PassUpdate(context.ExecutionFlags) is not (PassUpdateOption.AlwaysPasses or PassUpdateOption.CurrentlyPasses)) 
            return;
        
        int length = _nodes.Length;

        if (length == 1)
        {
            _nodes[0].Update(context);
        }
        
        ReadOnlySpan<IWrappedNode> nodes = _nodes;
        for (int i = 0; i < length; i++)
        {
            nodes[i].Update(context);
        }
    }
}
