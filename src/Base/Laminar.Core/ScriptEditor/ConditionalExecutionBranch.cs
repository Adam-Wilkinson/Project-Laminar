using System;
using Laminar.Contracts.NodeSystem;
using Laminar.PluginFramework.NodeSystem;

namespace Laminar.Core.ScriptEditor;

public class ConditionalExecutionBranch
{
    private readonly INodeWrapper[] _nodes;
    private readonly Func<bool> _isActiveCheck;

    public ConditionalExecutionBranch(INodeWrapper[] nodes, Func<bool> isActiveCheck)
    {
        _isActiveCheck = isActiveCheck;
        _nodes = nodes;
    }

    public void Execute(LaminarExecutionContext context)
    {
        if (_isActiveCheck())
        {
            Span<INodeWrapper> nodes = _nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].Update(context);
            }
        }
    }
}
