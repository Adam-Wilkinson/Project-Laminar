using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct DeleteNodeAction(INodeContainer nodeContainer, INodeCollection nodes) : IUserAction
{
    public INodeContainer NodeContainer { get; } = nodeContainer;
    
    public bool CanExecute { get; } = nodes.ContainsNode(nodeContainer);

    public Task<IUserActionResult> Execute() => Task.FromResult(nodes.DeleteNode(NodeContainer)
        ? IUserActionResult.Success(new AddNodeAction(NodeContainer, nodes))
        : IUserActionResult.Error(new NodeTreeDoesNotContainNodeException(NodeContainer)));

    public override string ToString() => $"Delete Node: {NodeContainer}";
}
