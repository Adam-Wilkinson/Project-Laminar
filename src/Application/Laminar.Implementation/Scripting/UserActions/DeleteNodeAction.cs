using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.UserActions;

internal readonly struct DeleteNodeAction(INodeContainer nodeContainer, INodeCollection nodes) : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public Task<IUserActionExecutionOutcome> Execute() => Task.FromResult(nodes.DeleteNode(NodeContainer)
        ? IUserActionExecutionOutcome.Success(new AddNodeAction(NodeContainer, nodes))
        : IUserActionExecutionOutcome.Error(new NodeTreeDoesNotContainNodeException(NodeContainer)));

    public override string ToString() => $"Delete Node: {NodeContainer}";
}
