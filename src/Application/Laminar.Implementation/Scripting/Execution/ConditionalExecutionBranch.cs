using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.PluginFramework.NodeSystem;
using Laminar.PluginFramework.NodeSystem.Connectors;

namespace Laminar.Implementation.Scripting.Execution;

public class ConditionalExecutionBranch : IConditionalExecutionBranch
{
    private readonly INodeContainer[] _nodes;
    private readonly IOutputConnector? _startingConnector;

    public ConditionalExecutionBranch(INodeContainer[] nodes)
    {
        _nodes = nodes;
    }

    public ConditionalExecutionBranch(INodeContainer[] nodes, IOutputConnector startingConnector)
    {
        _startingConnector = startingConnector;
        _nodes = nodes;
    }

    public ExecutionResult Execute(LaminarExecutionContext context)
    {
        if (_startingConnector is not null &&
            _startingConnector.PassUpdate(context.ExecutionFlags) is not (PassUpdateOption.AlwaysPasses or PassUpdateOption.CurrentlyPasses)) 
            return ExecutionResult.DidNotExecute;
        
        int length = _nodes.Length;

        if (length == 1)
        {
            if (_nodes[0].RuntimeNode is not { } runtimeNode) 
                return ExecutionResult.Error(new InvalidOperationException("The node does not have a runtime implementation"));
            
            runtimeNode.Update(context);
        }
        
        ReadOnlySpan<INodeContainer> nodes = _nodes;
        for (int i = 0; i < length; i++)
        {
            if (nodes[i].RuntimeNode is not { } runtimeNode)
            {
                return ExecutionResult.Error(new InvalidOperationException("The node does not have a runtime implementation"));
            }
            
            runtimeNode.Update(context);
        }

        return ExecutionResult.Success;
    }
}
