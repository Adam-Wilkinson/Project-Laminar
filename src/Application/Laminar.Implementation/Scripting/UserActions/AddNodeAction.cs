using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.UserActions;

internal readonly struct AddNodeAction(INodeContainer nodeContainer, INodeCollection nodes) : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public Task<IUserActionExecutionOutcome> Execute()
    {
        if (nodes.ContainsNode(NodeContainer))
        {
            return Task.FromResult(IUserActionExecutionOutcome.Error(new NodeTreeContainsNodeException(NodeContainer)));
        }
        
        nodes.AddNode(NodeContainer);
        return Task.FromResult(IUserActionExecutionOutcome.Success(new DeleteNodeAction(NodeContainer, nodes)));
    }

    public override string ToString()
    {
        return $"Add Node: {NodeContainer}";
    }
}
