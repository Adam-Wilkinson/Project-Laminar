using System;
using Laminar.Contracts.NodeSystem;
using Laminar.Contracts.NodeSystem.Execution;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Contracts.Connectors;

namespace Laminar.Core.ScriptEditor;

public class ConditionalExecutionBranch : IConditionalExecutionBranch
{
    private readonly INodeWrapper[] _nodes;
    private readonly IOutputConnector? _startingConnector;

    public ConditionalExecutionBranch(INodeWrapper[] nodes)
    {
        _nodes = nodes;
    }

    public ConditionalExecutionBranch(INodeWrapper[] nodes, IOutputConnector startingConnector)
    {
        _startingConnector = startingConnector;
        _nodes = nodes;
    }

    public void Execute(LaminarExecutionContext context)
    {
        if (_startingConnector is null || _startingConnector.PassUpdate(context.ExecutionFlags) is PassUpdateOption.AlwaysPasses or PassUpdateOption.CurrentlyPasses)
        {
            Span<INodeWrapper> nodes = _nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Update(context);
            }
        }
    }
}
