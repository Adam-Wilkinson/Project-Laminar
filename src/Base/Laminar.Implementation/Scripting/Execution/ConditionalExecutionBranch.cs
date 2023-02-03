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
        if (_startingConnector is null || _startingConnector.PassUpdate(context.ExecutionFlags) is PassUpdateOption.AlwaysPasses or PassUpdateOption.CurrentlyPasses)
        {
            Span<IWrappedNode> nodes = _nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Update(context);
            }
        }
    }
}
