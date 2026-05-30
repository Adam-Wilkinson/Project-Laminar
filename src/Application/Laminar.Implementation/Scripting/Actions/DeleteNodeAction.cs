using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct DeleteNodeAction(IWrappedNode node, INodeTree nodeTree) : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute { get; } = nodeTree.Nodes.Contains(node);

    public Task<IUserActionResult> Execute()
    {
        if (!nodeTree.DeleteNode(Node))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeDoesNotContainNodeException(Node)));
        }
        
        return Task.FromResult(IUserActionResult.Success(new AddNodeAction(Node, nodeTree)));
    }
    
    public override string ToString() => $"Delete Node: {Node}";
}
