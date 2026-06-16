using Laminar.Contracts.Base.ActionSystem;
using Laminar.Contracts.Scripting.Execution;
using Laminar.Contracts.Scripting.NodeWrapping;
using Laminar.Domain.Exceptions;

namespace Laminar.Implementation.Scripting.Actions;

internal readonly struct DeleteNodeAction(IWrappedNode node, IWritableNodeTree writableNodeTree) : IUserAction
{
    public IWrappedNode Node { get; } = node;
    
    public bool CanExecute { get; } = writableNodeTree.Nodes.Contains(node);

    public Task<IUserActionResult> Execute()
    {
        if (!writableNodeTree.DeleteNode(Node))
        {
            return Task.FromResult(IUserActionResult.Error(new NodeTreeDoesNotContainNodeException(Node)));
        }
        
        return Task.FromResult(IUserActionResult.Success(new AddNodeAction(Node, writableNodeTree)));
    }
    
    public override string ToString() => $"Delete Node: {Node}";
}
